using System.Linq;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameEvents {
    public class WaitChoice : MonoBehaviour {
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private BaseGameEvent m_ChoiceEvent;
        [SerializeField] private float m_DefaultDuration = 1;

        private DelaySequence m_Delay;

        private void Start() {
            m_StoryTeller.OnChoicesReadyAndWaiting += OnChoicesReadyAndWaiting;
            m_StoryTeller.OnChosen += OnChosen;
        }

        private void OnChosen() {
            m_Delay?.Cancel("A different choice has been made.", this);
        }

        private void OnChoicesReadyAndWaiting() {
            Restart("New choices available.");
            m_Delay?.Cancel("New choices available.", this);
            StoryChoice choice = m_StoryTeller.GetChoice(m_ChoiceEvent);

            if (choice != null) {
                string durationString = choice.ChoiceParams.FirstOrDefault();
                float duration = m_DefaultDuration;

                if (float.TryParse(durationString, out float parsedDuration)) {
                    duration = parsedDuration;
                }

                StoryDebug.Log($"Found [{choice.Text}] choice. Waiting for " + duration);

                if (duration > 0) {
                    m_Delay = Delay.For(duration, this).Then(m_ChoiceEvent.Raise);
                } else {
                    m_ChoiceEvent.Raise();
                }
            }
        }

        public void Stop(string reason) {
            m_Delay?.Cancel(reason, this);
        }

        public void Restart(string reason) {
            Stop(reason);

            StoryChoice choice = m_StoryTeller.GetChoice(m_ChoiceEvent);

            if (choice != null) {
                float duration = m_DefaultDuration;

                string durationString = choice.ChoiceParams.FirstOrDefault();

                if (float.TryParse(durationString, out float parsedDuration)) {
                    duration = parsedDuration;
                }

                StoryDebug.Log($"Found [{choice.Text}] choice. Waiting for " + duration);

                if (duration > 0) {
                    m_Delay = Delay.For(duration, this).Then(m_ChoiceEvent.Raise);
                } else {
                    m_ChoiceEvent.Raise();
                }
            }
        }
    }
}