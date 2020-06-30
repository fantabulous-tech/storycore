using System;
using System.Linq;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Choices {
    [CreateAssetMenu(menuName = "Choices/WaitChoice", fileName = "WaitChoiceHandler", order = 0)]
    public class WaitChoiceHandler : ChoiceHandler {
        [SerializeField] private float m_DefaultDuration = 1;

        [NonSerialized] private DelaySequence m_Delay;
        [NonSerialized] private float m_StartTime;
        [NonSerialized] private float m_Duration;

        public override void ReadyAndWaiting(StoryChoice choice) {
            base.ReadyAndWaiting(choice);
            m_StartTime = Time.time;
            m_Delay?.Cancel("New choices available.", this);

            string durationString = choice.ChoiceParams.FirstOrDefault();
            m_Duration = m_DefaultDuration;

            if (float.TryParse(durationString, out float parsedDuration)) {
                m_Duration = parsedDuration;
            }

            StoryDebug.Log($"Found [{choice.Text}] choice. Waiting for " + m_Duration);

            if (m_Duration > 0) {
                m_Delay = Delay.For(m_Duration, this).Then(Choose);
            } else {
                Choose();
            }
        }

        protected override void PauseInternal() {
            base.PauseInternal();
            Stop("Choice paused.");
            StoryDebug.Log($"WaitChoiceHandler: Pausing {name}", this);
        }

        protected override void ResumeInternal() {
            base.ResumeInternal();

            StoryDebug.Log($"WaitChoiceHandler: Resuming {name}", this);
            if (Time.time - m_StartTime > m_Duration) {
                Choose();
            } else {
                m_Delay = Delay.For(m_Duration - (Time.time - m_StartTime), this).Then(Choose);
            }
        }

        public override void Cancel() {
            base.Cancel();
            Stop("A different choice has been made.");
        }

        private void Stop(string reason) {
            m_Delay?.Cancel(reason, this);
        }
    }
}