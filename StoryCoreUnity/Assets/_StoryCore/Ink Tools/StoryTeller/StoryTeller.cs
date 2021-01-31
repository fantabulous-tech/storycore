using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreUtils;
using Ink.Runtime;
using CoreUtils.AssetBuckets;
using StoryCore.Choices;
using StoryCore.Commands;
using CoreUtils.GameEvents;
using CoreUtils.GameVariables;
using StoryCore.Characters;
using StoryCore.SaveLoad;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRSubtitles;

namespace StoryCore {
    public class StoryTeller : MonoBehaviour {
        [SerializeField] private TextAsset m_InkJson;
        [SerializeField] private SubtitleUI m_PromptUI;
        [SerializeField, AutoFillAsset] private GameVariableBool m_OptionSubtitles;
        [SerializeField, AutoFillAsset(DefaultName = "Restart")] private GameEvent m_RestartEvent;
        [SerializeField] private string m_RestartStoryPath = "load_game";
        [SerializeField, AutoFillAsset] private GameVariableChoice m_CurrentChoice;
        [SerializeField, AutoFillAsset] private GameVariableString m_FocusedCharacterName;
        [SerializeField, AutoFillAsset] private CharacterBucket m_CharacterBucket;
        [SerializeField, AutoFillAsset] private TextReplacementConfig m_TextReplacement;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField] private AbstractLineSequenceProvider m_CustomDialogLineProvider;

        private readonly LinkedList<ISequence> m_SequenceQueue = new LinkedList<ISequence>();
        private ISequence m_CurrentSequence;
        private StoryChoice m_NextChoice;
        private bool m_Complete;
        private Story m_Story;
        private BaseCharacter m_FocusedCharacter;
        private string m_LastFocusedCharacterName;
        private string m_LastSection;
        private string m_LastPath;

        public SubtitleUI PromptUI => m_PromptUI;
        public string CurrentStoryPath => m_LastPath;
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

        public string FocusedCharacterName {
            get {
                return m_FocusedCharacterName.Value;
            }
        }

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

            ChoiceManager.ChoiceEvent += TryChoice;
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
            StartStory();
        }

        private void OnDisable() {
            if (AppTracker.IsQuitting) {
                return;
            }
            
            ChoiceManager.ChoiceEvent -= TryChoice;
            AllChoices.Clear();
            CurrentChoices.Clear();
            Story = null;
        }

        private void Update() {
            if (m_Complete || AppTracker.IsQuitting) {
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

        private void TryChoice(string choiceKey) {
            if (CurrentChoices.Count == 0) {
                StoryDebug.Log($"StoryTeller: No choices currently available, so we can't try '{choiceKey}'");
                return;
            }

            StoryChoice choice = CurrentChoices.FirstOrDefault(c => c.Key.Contains(choiceKey, StringComparison.OrdinalIgnoreCase));
            if (choice != null) {
                StoryDebug.Log($"Choosing '{choice.Key}' match found for '{choiceKey}'");
                choice.Choose();
                return;
            }

            Debug.LogWarning(string.Format("No choice matching '{0}' found. (options = {1})", choiceKey, CurrentChoices.AggregateToString(c => c.Key)));
        }

        private void StartStory() {
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

            GetNextQueue("Starting story.");

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

        public void JumpStory(string storyPath) {
            StoryDebug.Log($"Jumping story to {storyPath}.", this);
            CancelQueue();
            m_Story.ChoosePathString(storyPath);
            
            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("none").Then(() => {
                m_Complete = false;
                GetNextQueue($"Jumping story to {storyPath}.");
            });
        }

        public void LoadStory() {
            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("none").Then(() => GetNextQueue($"Loading story."));
        }

        private void GetNextQueue(string reason) {
            if (!CanContinue) {
                Debag.LogErrorOnce($"StoryTeller can't get new sequences yet. This shouldn't happen. (Still have choices left?) Queue reason: {reason}", this);
                return;
            }

            StoryDebug.Log("Story Queue Updating: " + reason);

            CancelQueue();

            m_SequenceQueue.AddLast(new ActionSequence(EnableInterruptChoices));
            
            DialogLineSequence lastLine = null;
            LinkedListNode<ISequence> lastLineNode = null;
            RaiseUpdatingQueue();

            while (CanContinue) {
                // Check for the current path before and after the 'continue' as it is something null after 'Continue' is called.
                m_LastPath = Story.state.currentPathString;
                string text = Story.Continue().Trim();
                string path = Story.state.currentPathString;

                if (path.IsNullOrEmpty()) {
                    path = m_LastPath;
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

                    // Only track the 'last line' if it comes after all commands.
                    if (commandSequence.AllowsChoices) {
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

            while (blockNode != null && AllowsChoices(blockNode.Value, lastLine)) {
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
            if (m_CurrentSequence != null) {
                m_CurrentSequence.Cancel();
                m_CurrentSequence = null;
            }

            while (m_SequenceQueue.Any()) {
                m_SequenceQueue.First().Cancel();
                m_SequenceQueue.RemoveFirst();
            }
        }

        private static bool AllowsChoices(ISequence sequence, DialogLineSequence lastLine) {
            return sequence == lastLine || sequence.AllowsChoices && !(sequence is DialogLineSequence);
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

            // TODO: Figure out a way for choices to supply their own DelaySequences
            // so responses to choices wait for any appropriate choice events to finish.
            // ChoiceManager.GetChoiceDelay(choice).Then(() => {
            StoryDebug.Log($"Choosing {choice}!");
            Story.ChooseChoiceIndex(choice.Index);
            m_CurrentChoice.Value = choice;
            RaiseOnChosen();
            GetNextQueue($"Choice {choice} made. (index {choice.Index})");
            // });
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

        public bool IsValidChoice(BaseGameEvent gameEvent) {
            return ChoiceManager.Exists && CurrentChoices.Any(c => c.IsValidChoice(ChoiceManager.Choices[gameEvent]));
        }

        private string ChoiceInfo {
            get {
                return CurrentChoices.AggregateToString(c => c.Key);
            }
        }

        public StoryChoice GetChoice(BaseGameEvent choiceEvent) {
            return CurrentChoices.FirstOrDefault(c => c.IsValidChoice(ChoiceManager.Choices[choiceEvent]));
        }

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