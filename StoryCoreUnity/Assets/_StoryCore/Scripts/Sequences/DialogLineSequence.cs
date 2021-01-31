using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using VRSubtitles;
using Object = UnityEngine.Object;

namespace StoryCore {
    public class DialogLineSequence : ISequence {
        
        protected readonly StoryTeller m_StoryTeller;
        protected string m_Text;
        protected readonly string m_Section;

        private DelaySequence m_Delay;
        private Subtitle m_Subtitle;
        private bool m_Interrupted;

        public bool IsComplete { get; protected set; }
        public bool AllowsChoices => true;

        public bool DisplayChoicePrompt { private get; set; }

        protected virtual bool UseSubtitles => !m_Text.IsNullOrEmpty();

        public DialogLineSequence(StoryTeller storyTeller, string text, string section) {
            m_StoryTeller = storyTeller;
            m_Text = text;
            m_Section = section;
        }

        public virtual void Start() {

            // If there is no text, then we were just here to play the audio.
            // Let's set ourselves as done and bail.

            if (m_Text.IsNullOrEmpty() || m_Interrupted) {
                IsComplete = true;
                return;
            }

            float duration = GetDuration();

            if (UseSubtitles) {
                m_Subtitle = SubtitleDirector.ShowNow(
                    m_Text,
                    template: DisplayChoicePrompt ? m_StoryTeller.PromptUI : null,
                    speaker: m_StoryTeller.SubtitlePoint,
                    duration: DisplayChoicePrompt ? float.MaxValue : duration
                );
            }

            m_Delay = Delay.For(duration, m_StoryTeller).Then(OnComplete);
        }

        protected virtual float GetDuration() {
            // If we aren't using subtitles... then show a warning and move on.
            if (!UseSubtitles) {
                Debug.LogWarningFormat(m_StoryTeller, "Got an empty subtitle for some reason. Skipping to next early.");
                return 0;
            }

            return SubtitleDirector.GetDuration(m_Text);
        }

        protected virtual void OnComplete() {
            // Log("Subtitles completed: '" + m_Text + "'");
            IsComplete = true;
        }

        public virtual void Interrupt() {
            m_Interrupted = true;
            m_Delay?.Complete();
        }

        public void Cancel() {
            m_Delay?.Cancel("Dialog line cancelled.", m_StoryTeller);
        }

        protected void Log(string log, Object context = null) {
            StoryDebug.Log(log, context);
        }

        public override string ToString() {
            return $"{base.ToString()}: {m_Text}";
        }
    }
}