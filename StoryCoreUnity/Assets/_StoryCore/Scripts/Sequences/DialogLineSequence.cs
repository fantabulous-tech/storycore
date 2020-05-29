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

        public bool IsComplete { get; protected set; }
        public bool HasChoice { private get; set; }

        protected virtual bool UseSubtitles => !m_Text.IsNullOrEmpty();

        public DialogLineSequence(StoryTeller storyTeller, string text, string section) {
            m_StoryTeller = storyTeller;
            m_Text = text;
            m_Section = section;
        }

        public void OnQueue() {
            // Do nothing when queued. Should happen on Start().
        }

        public virtual void Start() {
            // Enable all choices when starting the last story line.
            if (HasChoice) {
                m_StoryTeller.EnableAllChoices(false);
            }

            // If there is no text, then we were just here to play the audio.
            // Let's set ourselves as done and bail.

            if (m_Text.IsNullOrEmpty()) {
                IsComplete = true;
                return;
            }

            float duration = GetDuration();

            if (UseSubtitles) {
                m_Subtitle = SubtitleDirector.ShowNow(
                    m_Text,
                    template: HasChoice ? m_StoryTeller.PromptUI : null,
                    speaker: m_StoryTeller.AttentionPoint,
                    duration: HasChoice ? float.MaxValue : duration
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

        public void Cancel() {
            if (m_Delay != null) {
                m_Delay.Cancel("Script line cancelled.", m_StoryTeller);
            }
        }

        protected void Log(string log, Object context = null) {
            m_StoryTeller.Log(log, context);
        }
    }
}