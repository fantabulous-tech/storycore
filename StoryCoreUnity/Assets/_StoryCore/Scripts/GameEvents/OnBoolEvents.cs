using System;
using StoryCore.GameVariables;
using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.GameEvents {
    public class OnBoolEvents : MonoBehaviour {
        [SerializeField] private GameVariableBool m_Bool;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;

        private void Awake() {
            if (m_Bool == null) {
                Debug.LogWarningFormat(this, $"No bool assigned to {name}");
                return;
            }

            m_Bool.Changed += OnBoolChanged;
        }

        private void OnDestroy() {
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