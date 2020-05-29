using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.GameEvents {
    public class OnGameEventDoAction : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_GameEvent;

        public UnityEvent Action;

        private void Awake() {
            if (m_GameEvent != null) {
                m_GameEvent.GenericEvent += OnEvent;
            } else {
                Debug.LogWarningFormat(this, "GameEvent not set.");
            }
        }

        private void OnDestroy() {
            if (m_GameEvent != null) {
                m_GameEvent.GenericEvent -= OnEvent;
            }
        }

        private void OnEvent() {
            Action.Invoke();
        }
    }
}