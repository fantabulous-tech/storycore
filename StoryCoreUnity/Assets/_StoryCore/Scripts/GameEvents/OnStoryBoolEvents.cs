using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.GameEvents {
    public class OnStoryBoolEvents : MonoBehaviour {
        [SerializeField] private GameVariableStoryBool m_Bool;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;

        private void OnEnable() {
            if (m_Bool == null) {
                Debug.LogWarningFormat(this, $"No bool assigned to {name}");
                return;
            }

            m_Bool.Changed += OnBoolChanged;
            OnBoolChanged(m_Bool.Value);
        }

        private void OnDisable() {
            if (m_Bool != null) {
                m_Bool.Changed -= OnBoolChanged;
            }
        }

        private void OnBoolChanged(bool value) {
            if (value) {
                OnTrue.Invoke();
            } else {
                OnFalse.Invoke();
            }
        }
    }
}