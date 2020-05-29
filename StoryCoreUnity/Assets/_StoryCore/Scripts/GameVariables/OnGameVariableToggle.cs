using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.GameVariables {
    public class OnGameVariableToggle : MonoBehaviour {
        [SerializeField] private GameVariableBool m_ToggleVariable;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;
        public UnityEvent<bool> OnChanged;

        private void Awake() {
            if (m_ToggleVariable != null) {
                m_ToggleVariable.Changed += OnChange;
                OnChange(m_ToggleVariable.Value);
            }
        }

        private void OnChange(bool value) {
            if (value) {
                OnTrue.Invoke();
            } else {
                OnFalse.Invoke();
            }

            OnChanged.Invoke(value);
        }
    }
}