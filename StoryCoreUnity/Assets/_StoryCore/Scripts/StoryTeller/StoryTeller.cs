using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ink.Runtime;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using StoryCore.GameEvents;
using StoryCore.GameVariables;
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
        [SerializeField] private string m_RestartStoryPath = "game_start";
        [SerializeField, AutoFillAsset] private GameVariableChoice m_CurrentChoice;
        [SerializeField, AutoFillAsset] private GameVariableString m_FocusedCharacterName;
        [SerializeField, AutoFillAsset] private CharacterBucket m_CharacterBucket;
        [SerializeField, AutoFillAsset] private TextReplacementConfig m_TextReplacement;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField] private AbstractLineSequenceProvider m_CustomDialogLineProvider;
        [SerializeField] private bool m_StartStoryOnEnable = true;

        private readonly Queue<ISequence> m_SequenceQueue = new Queue<ISequence>();
        private ISequence m_CurrentSequence;
        private StoryChoice m_NextChoice;
        private bool m_Complete;
        private Story m_Story;
        private BaseCharacter m_FocusedCharacter;
        private string m_LastFocusedCharacterName;

        public SubtitleUI PromptUI => m_PromptUI;
        private bool PlayingSequence => m_CurrentSequence != null && !m_CurrentSequence.IsComplete;
        public bool UseSubtitles => m_OptionSubtitles.Value;
        private static string OverrideInkPath => Application.persistentDataPath + "/_game.json";
        private static string OverrideInstructionsPath => Application.persistentDataPath + "/override instructions.txt";
        private string InkJsonText {
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

        public Transform AttentionPoint => FocusedCharacter ? FocusedCharacter.AttentionPoint : null;
        public Transform SubtitlePoint => FocusedCharacter ? FocusedCharacter.SubtitlePoint : null;

        public Story Story {
            get => m_Story;
            private set {
                m_Story = value;
                OnStoryReady?.Invoke();
            }
        }
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

        public event Action OnStoryReady;
        public event Action OnNext;
        public event Action OnChoicesReady;
        public event Action OnChoicesReadyAndWaiting;
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

            if (m_StartStoryOnEnable) {
                StartStory();
            }
        }

        private void OnDisable() {
            ChoiceManager.ChoiceEvent -= TryChoice;
            CurrentChoices.Clear();
            Story = null;
        }

        private void Update() {
            if (m_Complete) {
                return;
            }
            if (m_Story != null) {
                UpdateSequences();
                UpdateChoiceDelay();
            }
        }

        private void UpdateChoiceDelay() {
            if (m_NextChoice != null && m_CurrentSequence is ReadyForChoiceSequence choiceSequence) {
                choiceSequence.IsComplete = true;
                ChooseInternal(m_NextChoice);
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
                m_CurrentSequence = m_SequenceQueue.Dequeue();
                m_CurrentSequence.Start();
            }

            RaiseOnNext();
        }

        private void TryChoice(string choiceKey) {
            if (CurrentChoices.Count == 0) {
                StoryDebug.Log($"StoryTeller: No choices currently available, so we can't try '{choiceKey}'");
                return;
            }

            StoryChoice choice = CurrentChoices.FirstOrDefault(c => c.Text.Contains(choiceKey, StringComparison.OrdinalIgnoreCase));
            if (choice != null) {
                StoryDebug.Log($"Choosing '{choice.Text}' match found for '{choiceKey}'");
                choice.Choose();
                return;
            }

            Debug.LogWarning(string.Format("No choice matching '{0}' found. (options = {1})", choiceKey, CurrentChoices.AggregateToString(c => c.Text)));
        }

        public void StartStory() {
            if (!m_InkJson && !File.Exists(OverrideInkPath)) {
                Debug.LogErrorFormat(this, "Cannot start story. No Ink JSON text file assigned.");
                return;
            }

            m_Complete = false;
            Story = new Story(InkJsonText);
            Story.BindExternalFunction("isDebug", () => Application.isEditor && StoryCoreSettings.UseDebugInInk);

            // Load up all the saved variables (story variables should get set in the story too).
            SaveLoadVariables.LoadAll();

            GetNextQueue("Starting story.");

            // Now that initial variable notifications have run, we can subscribe to future events.
            SaveLoadVariables.SubscribeAll();
        }

        public void RestartStory() {
            RestartStory(m_RestartStoryPath);
        }

        public void RestartStory(string storyPath) {
            IStoryVariable[] storyVariables = SaveLoadVariables.SavedVariables.OfType<IStoryVariable>().ToArray();

            // Stop listening so we don't raise the variable changes when resetting the state.
            storyVariables.ForEach(v => v.Unsubscribe());

            StoryDebug.Log("Resetting game state.");
            m_Story.ResetState();

            // Set the story to use the saved story variables and re-subscribe.
            storyVariables.ForEach(v => {
                v.SetInStory();
                v.Subscribe();
            });

            m_Story.variablesState["hasRestarted"] = true;
            m_Story.ChoosePathString(storyPath);

            // Add loading an empty scene as the first command.
            CommandSceneHandler.LoadScene("empty").Then(() => GetNextQueue("Restarting story."));
        }

        private string m_LastSection;

        private void GetNextQueue(string reason) {
            StoryDebug.Log("Story Queue Updating: " + reason);

            if (m_CurrentSequence != null) {
                m_CurrentSequence.Cancel();
                m_CurrentSequence = null;
            }

            while (m_SequenceQueue.Any()) {
                m_SequenceQueue.Dequeue().Cancel();
            }

            DialogLineSequence lastLine = null;

            while (CanContinue) {
                // Check for the current path before and after the 'continue' as it is something null after 'Continue' is called.
                string lastPath = Story.state.currentPathString;
                string text = Story.Continue().Trim();
                string path = Story.state.currentPathString;

                if (path.IsNullOrEmpty()) {
                    path = lastPath;
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
                    CommandSequence commandSequence = new CommandSequence(this, text, Story.currentTags);
                    m_SequenceQueue.Enqueue(commandSequence);
                    commandSequence.OnQueue();

                    // If this is a 'notify' command, then it probably is used as the 'last line',
                    // so clear any previous 'lastLine's so the subtitle doesn't stay on.
                    // TODO: Figure out a better way to check for this.
                    if (text.Contains("/notify")) {
                        lastLine = null;
                    }

                    continue;
                }

                text = m_TextReplacement.Convert(text);
                lastLine = CreateDialogLine(text, m_LastSection);
                m_SequenceQueue.Enqueue(lastLine);
                lastLine.OnQueue();
            }

            if (lastLine != null && CanChooseUI) {
                lastLine.HasChoice = true;
            }

            EnableInterruptChoices();

            if (HasChoices) {
                ReadyForChoiceSequence readyForChoiceSequence = new ReadyForChoiceSequence(this);
                m_SequenceQueue.Enqueue(readyForChoiceSequence);
                readyForChoiceSequence.OnQueue();
            } else {
                EndStorySequence endStorySequence = new EndStorySequence(this);
                m_SequenceQueue.Enqueue(endStorySequence);
                endStorySequence.OnQueue();
            }
        }

        private DialogLineSequence CreateDialogLine(string text, string section) {
            return m_CustomDialogLineProvider ? m_CustomDialogLineProvider.CreateDialogLine(this, text, section) : new DialogLineSequence(this, text, section);
        }

        private void EnableInterruptChoices() {
            CurrentChoices.Clear();

            for (int i = 0; i < Story.currentChoices.Count; i++) {
                Choice choice = Story.currentChoices[i];
                if (CanInterrupt(choice)) {
                    CurrentChoices.Add(new StoryChoice(choice, this));
                }
            }

            RaiseOnChoicesReady();
        }

        public void EnableAllChoices(bool waiting) {
            CurrentChoices = Story.currentChoices.Select((c, i) => new StoryChoice(c, this)).ToList();
            RaiseOnChoicesReady();
            if (waiting) {
                RaiseOnChoicesReadyAndWaiting();
            }
        }

        private static bool CanInterrupt(Choice choice) {
            return choice.text.EndsWith("!");
        }

        public static float GetPunctuationPause(string text) {
            return text.IsNullOrEmpty() ? 0 : GetPunctuationPause(text.Trim().Last());
        }

        private static float GetPunctuationPause(char c) {
            // If the last character was a letter, then the break is
            // a continuous sentence and shouldn't have any pause.
            if (char.IsLetter(c)) {
                return 0;
            }

            // Otherwise, use longer pause for special 'pause' punctuation
            // or a short pause otherwise.
            switch (c) {
                case '.':
                case '?':
                case '"':
                case '\'':
                case '\\':
                case '~':
                case ';':
                case ':':
                case ')':
                case ']':
                    return 0.8f;
                case '-':
                case '/':
                    return 0;
                default:
                    return 0.4f;
            }
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

        public bool Choose(int choice) {
            StoryChoice storyChoice = CurrentChoices.ElementAtOrDefault(choice);

            if (storyChoice == null) {
                Debug.LogWarning($"Choice #{choice} was not found. Current choices: {CurrentChoices.AggregateToString()}", this);
                return false;
            }

            Choose(storyChoice);
            return true;
        }

        public void Choose(StoryChoice choice) {
            // Clear out any more items in the queue (for interrupt)
            m_SequenceQueue.Clear();

            // Clear out choices now that we have a choice.
            CurrentChoices.Clear();

            if (m_CurrentSequence is ReadyForChoiceSequence) {
                // If we are ready to choose, make the choice now.
                ChooseInternal(choice);
            } else {
                // Otherwise, set the choice's index while we wait for the sequence to finish.
                m_NextChoice = choice;
                RaiseOnChoosing(choice);
            }
        }

        private void ChooseInternal(StoryChoice choice) {
            m_NextChoice = null;

            RaiseOnChoosing(choice);
            SubtitleDirector.FadeOut();

            // TODO: Figure out a way for choices to supply their own DelaySequences
            // so responses to choices wait for any appropriate choice events to finish.

            //Delay.For(1, this).Then(() => {
            Story.ChooseChoiceIndex(choice.Index);
            EnableAllChoices(false);
            m_CurrentChoice.Value = choice;
            RaiseOnChosen();
            GetNextQueue("Choice pause complete.");
            //});
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

        public void RaiseOnChoicesReadyAndWaiting() {
            if (CurrentChoices.Count > 0) {
                OnChoicesReadyAndWaiting?.Invoke();
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
            return CurrentChoices.Any(c => c.IsValidChoice(ChoiceManager.Choices[gameEvent]));
        }

        private string ChoiceInfo {
            get {
                return CurrentChoices.AggregateToString(c => c.Text);
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