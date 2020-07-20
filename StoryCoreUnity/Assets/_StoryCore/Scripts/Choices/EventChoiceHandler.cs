using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.Choices {
    [CreateAssetMenu(menuName = "Choices/EventChoice", fileName = "ChoiceEvent", order = 0)]
    public class EventChoiceHandler : ChoiceHandler {
        [SerializeField] private GameEvent m_GameEvent;

        private bool m_ChosenWhilePaused;
        
        public override void Ready(StoryChoice choice) {
            base.Ready(choice);
            m_GameEvent.GenericEvent += OnGameEvent;
            m_ChosenWhilePaused = false;
        }

        private void OnGameEvent() {
            if (IsPaused) {
                m_ChosenWhilePaused = true;
            } else {
                Choose();
            }
        }

        protected override void ResumeInternal() {
            base.ResumeInternal();
            if (m_ChosenWhilePaused) {
                Choose();
            }
        }

        protected override void ChooseInternal() {
            base.ChooseInternal();
            m_GameEvent.GenericEvent -= OnGameEvent;
        }

        protected override void CancelInternal(StoryChoice choice) {
            base.CancelInternal(choice);
            m_GameEvent.GenericEvent -= OnGameEvent;
            m_ChosenWhilePaused = false;
        }
    }
}