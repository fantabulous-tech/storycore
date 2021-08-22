using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreUtils;
using CoreUtils.AssetBuckets;
using CoreUtils.GameEvents;
using CoreUtils.GameVariables;
using Ink.Runtime;
using StoryCore.Characters;
using StoryCore.Choices;
using StoryCore.Commands;
using StoryCore.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRSubtitles;

namespace StoryCore {
    public class StoryTeller : MonoBehaviour {
        [SerializeField] private TextAsset m_InkJson;
        [SerializeField] private SubtitleUI m_PromptUI;
        [SerializeField, AutoFillAsset] private GameVariableBool m_OptionSubtitles;
        [SerializeField, AutoFillAsset(DefaultName = "Restart")] private GameEvent m_RestartEvent;
        [SerializeField] private string m_RestartStoryPath = "game_start";
        [SerializeField, AutoFillAsset] private GameVariableChoice m_CurrentChoice;
        [SerializeField, AutoFillAsset] private GameVariableString m_FocusedCharacterName;
        [SerializeField, AutoFillAsset] private CharacterBucket m_CharacterBucket;
        [SerializeField, AutoFillAsset] private TextReplacementConfig m_TextReplacement;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField] private AbstractLineSequenceProvider m_CustomDialogLineProvider;
        [SerializeField] private bool m_StartStoryOnEnable = true;

        private readonly LinkedList<ISequence> m_SequenceQueue = new LinkedList<ISequence>();
        private ISequence m_CurrentSequence;
        private StoryChoice m_NextChoice;
        private bool m_Complete;
        private Story m_Story;
        private BaseCharacter m_FocusedCharacter;
        private string m_LastFocusedCharacterName;
        private string m_LastSection;
        private bool m_Loading = true;

        public SubtitleUI PromptUI => m_PromptUI;
        public string CurrentStoryPath { get; private set; }
        private bool PlayingSequence => m_CurrentSequence != null && !m_CurrentSequence.IsComplete;
        public bool UseSubtitles => m_OptionSubtitles.Value;
        private static string OverrideInkPath => Application.persistentDataPath + "/_game.json";
        private static string OverrideInstructionsPath => Application.persistentDataPath + "/override instructions.txt";
        public string InkJsonText {
            get {
                bool hasOverride = File.Exists(OverrideInkPath);

                if (!hasOverride) {
                    return m_InkJson.text;
                }

                DateTime overrideDate = File.GetLastWriteTimeUtc(OverrideInkPath);

#if UNITY_EDITOR
                string originalPath = UnityEditor.AssetDatabase.GetAssetPath(m_InkJson);
#else
				string originalPath = "";
#endif
                if (File.Exists(originalPath) && File.GetLastWriteTimeUtc(originalPath) > overrideDate) {
                    Debug.LogWarning($"Skipping override .json because it is older than {originalPath}", m_InkJson);
                    return m_InkJson.text;
                }

                Debug.LogWarning($"Using override JSON file: {OverrideInkPath}");
                return File.ReadAllText(OverrideInkPath);
            }
        }

        private bool FocusIsOld => !m_FocusedCharacterName.Value.IsNullOrEmpty() && (m_FocusedCharacter == null || !m_FocusedCharacter.Name.Equals(m_FocusedCharacterName.Value, StringComparison.OrdinalIgnoreCase));

        public BaseCharacter FocusedCharacter {
            get {
                if (FocusIsOld) {
                    OnFocusChanged(m_FocusedCharacterName.Value);
                }
                return m_FocusedCharacter;
            }
        }

        public string FocusedCharacterName => m_FocusedCharacterName.Value;

        public Transform AttentionPoint => FocusedCharacter ? FocusedCharacter.AttentionPoint : null;
        public Transform SubtitlePoint => FocusedCharacter ? FocusedCharacter.SubtitlePoint : null;

        public Story Story {
            get => m_Story;
            private set {
                if (m_Story == value) {
                    return;
                }

                m_Story = value;
                OnStoryCreated?.Invoke(m_Story);
            }
        }
        public List<StoryChoice> AllChoices { get; private set; } = new List<StoryChoice>();
        public List<StoryChoice> CurrentChoices { get; private set; } = new List<StoryChoice>();
        public bool HasChoices => Story && Story.currentChoices.Count > 0;

        private bool CanContinue => Story && Story.canContinue;
        private bool CanChooseUI {
            get {
                if (!HasChoices) {
                    return false;
                }

                if (Story.currentChoices.Count > 1) {
                    return true;
                }

                return !Story.currentChoices[0].text.Contains("wait", StringComparison.OrdinalIgnoreCase) &&
                       !Story.currentChoices[0].text.Contains("continue", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanChoose(string choice) {
            return HasChoices && CurrentChoices.Exists(c => c.IsValidChoice(choice));
        }

        public event Action<Story> OnStoryCreated;
        public event Action OnStoryReadyToLoad;
        public event Action OnStoryReady;
        public event Action OnNext;
        public event Action OnQueueUpdating;
        public event Action OnChoicesReady;
        public event Action OnChoicesWaiting;
        public event Action<StoryChoice> OnChoosing;
        public event Action OnChosen;
        public event Action OnEnd;

        private void Awake() {
            m_StoryTellerLocator.Value = this;
        }

        private void OnEnable() {
            StoryDebug.Log("Persistent Data Path = " + Application.persistentDataPath);

            if (!File.Exists(OverrideInstructionsPath)) {
                File.WriteAllText(OverrideInstructionsPath, "Place a _game.json file here to override the default _game.json.");
            }

            if (m_RestartEvent) {
                m_RestartEvent.GenericEvent += RestartStory;
            }
            if (m_FocusedCharacterName) {
                m_FocusedCharacterName.Changed += OnFocusChanged;
            }
            if (m_CharacterBucket) {
                m_CharacterBucket.Added += OnCharacterAdded;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (AppTracker.IsPlaying && m_StartStoryOnEnable) {
                StartStory();
            }
        }

        private void OnDisable() {
            if (AppTracker.IsQuitting) {
                return;
            }

            AllChoices.Clear();
            CurrentChoices.Clear();
            Story = null;
        }

        private void Update() {
            if (m_Loading || m_Complete || AppTracker.IsQuitting) {
                return;
            }

            if (m_Story != null) {
                UpdateChoiceDelay();
                UpdateSequences();
            }
        }

        private void UpdateChoiceDelay() {
            if (m_NextChoice != null && m_CurrentSequence is WaitForChoiceSequence) {
                CompleteChoice();
            }
        }

        private void UpdateSequences() {
            if (PlayingSequence) {
                return;
            }

            if (m_SequenceQueue.Count == 0) {
                GetNextQueue("Current queue is empty.");
            }

            while (m_SequenceQueue.Count > 0 && (m_CurrentSequence == null || m_CurrentSequence.IsComplete)) {
                m_CurrentSequence = m_SequenceQueue.First();
                m_SequenceQueue.RemoveFirst();
                // StoryDebug.Log($"StoryTeller: Starting {m_CurrentSequence}");
                m_CurrentSequence.Start();
            }

            RaiseOnNext();
        }

        public void StartStory() {
            if (!m_InkJson && !File.Exists(OverrideInkPath)) {
                Debug.LogErrorFormat(this, "Cannot start story. No Ink JSON text file assigned.");
                return;
            }

            m_Complete = false;
            Story = new Story(InkJsonText);
            OnStoryReadyToLoad?.Invoke();
            OnStoryReady?.Invoke();

            // Load up all the saved variables (story variables should get set in the story too).
            Story.variablesState["debug"] = Application.isEditor && StoryCoreSettings.UseDebugInInk;

            if (Application.isEditor && StoryCoreSettings.UseDebugInInk && !StoryCoreSettings.DebugJump.IsNullOrEmpty()) {
                try {
                    Story.EvaluateFunction("setGender", "m");
                    JumpStory(StoryCoreSettings.DebugJump);
                }
                catch (Exception e) {
                    GetNextQueue($"Starting story. (Jump didn't work because {e.Message}.)");
                }
            } else {
                GetNextQueue("Starting story.");
            }

            // Now that initial variable notifications have run, we can subscribe to future events.
            SaveLoadVariables.SubscribeAll();
        }

        public void RestartStory() {
            RestartStory(m_RestartStoryPath);
        }

        public void RestartStory(string storyPath) {
            if (m_Story == null) {
                Debug.LogWarning("No story found. Can't restart.");
                return;
            }

            StoryDebug.Log("Resetting game state.");
            m_Story.ResetState();

            OnStoryReadyToLoad?.Invoke();
            OnStoryReady?.Invoke();

            Story.variablesState["debug"] = Application.isEditor && StoryCoreSettings.UseDebugInInk;

            m_Story.ChoosePathString(storyPath);
            CancelQueue();

            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("none").Then(() => {
                m_Complete = false;
                GetNextQueue("Restarting story.");
            });
        }

        public void SkipTo(string storyPath) {
            string save = m_Story.state.ToJson();
            try {
                m_Story.ChoosePathString(storyPath);
            }
            catch (Exception e) {
                Debug.LogWarning($"Can't skip to '{storyPath}' because {e.Message}");
                m_Story.state.LoadJson(save);
                throw;
            }

            StoryDebug.Log($"Skipping story to {storyPath}.", this);
            CancelQueue();
            m_Complete = false;
            GetNextQueue($"Jumping story to {storyPath}.");
        }

        public void Continue() {
            m_Complete = false;
            GetNextQueue($"Continuing story.");
        }

        public void JumpStory(string storyPath) {
            string save = m_Story.state.ToJson();
            try {
                m_Story.ChoosePathString(storyPath);
            }
            catch (Exception e) {
                Debug.LogWarning($"Can't choose {storyPath} because {e.Message}");
                m_Story.state.LoadJson(save);
                throw;
            }

            StoryDebug.Log($"Jumping story to {storyPath}.", this);
            CancelQueue();

            // Pause Update loop until empty scene loads.
            m_Loading = true;

            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("none").Then(() => {
                m_Complete = false;
                GetNextQueue($"Jumping story to {storyPath}.");
            });
        }

        public void LoadStory() {
            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("none").Then(() => GetNextQueue("Loading story."));
        }

        private void GetNextQueue(string reason) {
            if (!CanContinue) {
                Debug.LogWarning($"StoryTeller can't get new sequences. This shouldn't happen. Story Queue Updating: {reason}", this);
                // return;
            } else {
                StoryDebug.Log("Story Queue Updating: " + reason);
            }

            CancelQueue();
            m_Loading = false;
            m_SequenceQueue.AddLast(new ActionSequence(EnableInterruptChoices));

            DialogLineSequence lastLine = null;
            LinkedListNode<ISequence> lastLineNode = null;
            RaiseUpdatingQueue();

            while (CanContinue) {
                // Check for the current path before and after the 'continue' as it is something null after 'Continue' is called.
                CurrentStoryPath = Story.state.currentPathString;
                string text = Story.Continue().Trim();
                string path = Story.state.currentPathString;

                if (path.IsNullOrEmpty()) {
                    path = CurrentStoryPath;
                }

                if (!path.IsNullOrEmpty()) {
                    m_LastSection = path.Split('.').FirstOrDefault();
                } else {
                    Debug.LogWarning($"Current state path is null: '{Story.state.currentPathString}'");
                }

                if (text.IsNullOrEmpty()) {
                    //Log($"SKIP EMPTY TEXT: '{text}'");
                    continue;
                }

                if (text.StartsWith("/")) {
                    CommandSequence commandSequence = new CommandSequence(text, Story.currentTags);
                    m_SequenceQueue.AddLast(commandSequence);

                    // Only track the 'last line' if it comes after all non 'AllowChoices' commands.
                    if (!commandSequence.AllowsChoices) {
                        lastLineNode = null;
                    }

                    continue;
                }

                text = m_TextReplacement.Convert(text);
                lastLine = CreateDialogLine(text, m_LastSection);
                m_SequenceQueue.AddLast(lastLine);
                lastLineNode = m_SequenceQueue.Last;
            }

            // We can no longer continue. Let's create all the available choices at once.
            AllChoices = Story.currentChoices.Select((c, i) => new StoryChoice(c, this)).ToList();

            if (lastLineNode != null) {
                // If we have a last VO line, add choice UI
                lastLine.DisplayChoicePrompt = CanChooseUI;
            }

            LinkedListNode<ISequence> blockNode = m_SequenceQueue.Last;
            bool firstDialog = true;

            while (blockNode != null && (blockNode.Value.AllowsChoices || blockNode.Value is DialogLineSequence && firstDialog)) {
                if (blockNode.Value is DialogLineSequence) {
                    firstDialog = false;
                }

                blockNode = blockNode.Previous;
            }

            if (blockNode == null) {
                m_SequenceQueue.AddFirst(new ActionSequence(EnableAllChoices));
            } else {
                m_SequenceQueue.AddAfter(blockNode, new ActionSequence(EnableAllChoices));
            }

            if (HasChoices) {
                m_SequenceQueue.AddLast(new WaitForChoiceSequence(this, RaiseOnChoicesWaiting));
            } else {
                EndStorySequence endStorySequence = new EndStorySequence(this);
                m_SequenceQueue.AddLast(endStorySequence);
            }

            StoryDebug.Log($"New Sequences x{m_SequenceQueue.Count}: \n{m_SequenceQueue.AggregateToString("\n")}\n\n", this);
        }

        private void CancelQueue() {
            // Also cancel any choices.
            RaiseOnChosen();
            m_Loading = true;

            List<ISequence> cancelList = new List<ISequence>(m_SequenceQueue);
            if (m_CurrentSequence != null && !(m_CurrentSequence is WaitForChoiceSequence)) {
                cancelList.Insert(0, m_CurrentSequence);
            }

            if (cancelList.Any()) {
                StoryDebug.Log($"Queue cancelling {cancelList.Count} items:\n{cancelList.AggregateToString("\n")}");
            }

            if (m_CurrentSequence != null) {
                m_CurrentSequence.Cancel();
                m_CurrentSequence = null;
            }

            while (m_SequenceQueue.Any()) {
                m_SequenceQueue.First().Cancel();
                m_SequenceQueue.RemoveFirst();
            }
        }

        private void RaiseUpdatingQueue() {
            OnQueueUpdating?.Invoke();
        }

        private DialogLineSequence CreateDialogLine(string text, string section) {
            return m_CustomDialogLineProvider
                       ? m_CustomDialogLineProvider.CreateDialogLine(this, text, section)
                       : new DialogLineSequence(this, text, section);
        }

        private void EnableInterruptChoices() {
            CurrentChoices = AllChoices.Where(c => c.CanInterrupt).ToList();
            RaiseOnChoicesReady();
        }

        private void EnableAllChoices() {
            CurrentChoices = AllChoices.ToList();
            RaiseOnChoicesReady();
        }

        public void InterruptSequences() {
            if (m_CurrentSequence != null) {
                m_CurrentSequence.Interrupt();
            }
            m_SequenceQueue.ForEach(s => {
                if (s != null) {
                    s.Interrupt();
                }
            });
        }

        public bool Choose(string choice) {
            StoryChoice storyChoice = CurrentChoices.FirstOrDefault(c => c.IsValidChoice(choice));

            if (storyChoice == null) {
                Debug.LogWarning($"Choice '{choice}' was not found. Current choices: {CurrentChoices.AggregateToString()}", this);
                return false;
            }

            Choose(storyChoice);
            return true;
        }

        public bool Choose(int choiceIndex) {
            StoryChoice storyChoice = AllChoices.ElementAtOrDefault(choiceIndex);

            if (storyChoice == null) {
                Debug.LogWarning($"Choice #{choiceIndex} was not found. Current choices: {CurrentChoices.AggregateToString()}", this);
                return false;
            }

            if (!CurrentChoices.Contains(storyChoice)) {
                Debug.LogWarning($"Choice #{choiceIndex} was found, but not yet an available choice. Current choices: {CurrentChoices.AggregateToString()}", this);
                return false;
            }

            Choose(storyChoice);
            return true;
        }

        public void Choose(StoryChoice choice) {
            if (!CurrentChoices.Contains(choice)) {
                Debug.LogWarning($"Tried to select invalid choice: {choice}.");
                return;
            }

            // Clear out choices now that we have the next choice.
            AllChoices.ForEach(c => c.Disable());
            CurrentChoices.Clear();
            AllChoices.Clear();
            StartChoice(choice);

            if (m_CurrentSequence is WaitForChoiceSequence) {
                // If we are ready to choose, make the choice now.
                CompleteChoice();
            } else {
                // Otherwise, set the choice's index while we wait for the sequence to finish.
                StoryDebug.Log($"Waiting to choose {choice} / Current sequence: {m_CurrentSequence}");
            }
        }

        private void StartChoice(StoryChoice choice) {
            m_NextChoice = choice;
            RaiseOnChoosing(choice);
        }

        private void CompleteChoice() {
            StoryChoice choice = m_NextChoice;
            m_NextChoice = null;
            SubtitleDirector.FadeOut();
            ChoiceManager.GetChoiceDelay(choice).Then(() => {
                StoryDebug.Log($"Choosing {choice}!");
                Story.ChooseChoiceIndex(choice.Index);
                m_CurrentChoice.Value = choice;
                RaiseOnChosen();
                GetNextQueue($"Choice {choice} made. (index {choice.Index})");
            });
        }

        public void EndStory() {
            StoryDebug.Log("END!");
            RaiseOnEnd();
            m_Complete = true;
        }

        private void RaiseOnNext() {
            OnNext?.Invoke();
        }

        private void RaiseOnChoicesReady() {
            if (CurrentChoices.Count > 0) {
                OnChoicesReady?.Invoke();
                StoryDebug.Log($"Current Choices: {ChoiceInfo}");
            }
        }

        private void RaiseOnChoicesWaiting() {
            if (CurrentChoices.Count > 0) {
                OnChoicesWaiting?.Invoke();
                StoryDebug.Log($"Current Choices And Waiting: {ChoiceInfo}");
            }
        }

        private void RaiseOnChoosing(StoryChoice choice) {
            OnChoosing?.Invoke(choice);
        }

        private void RaiseOnChosen() {
            OnChosen?.Invoke();
        }

        private void RaiseOnEnd() {
            OnEnd?.Invoke();
        }

        private string ChoiceInfo {
            get {
                return CurrentChoices.AggregateToString(c => c.Key);
            }
        }

        public bool QueueDone => m_SequenceQueue.Count == 0 && (m_CurrentSequence == null || m_CurrentSequence.IsComplete);

        private void OnFocusChanged(string characterName) {
            BaseCharacter character = m_CharacterBucket.Get(characterName);
            if (character) {
                m_FocusedCharacter = character;
            } else {
                if (m_LastFocusedCharacterName != characterName) {
                    Debug.LogWarning($"Couldn't find '{characterName}'", this);
                }

                m_FocusedCharacter = null;
            }

            m_LastFocusedCharacterName = characterName;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (!m_FocusedCharacterName.Value.IsNullOrEmpty()) {
                OnFocusChanged(m_FocusedCharacterName.Value);
            }
        }

        private void OnCharacterAdded(BaseCharacter character) {
            if (m_FocusedCharacterName.Value.IsNullOrEmpty()) {
                m_FocusedCharacterName.Value = character.Name;
            }
        }

    }

    public abstract class AbstractLineSequenceProvider : ScriptableObject {
        public abstract DialogLineSequence CreateDialogLine(StoryTeller storyTeller, string text, string section);
    }
}