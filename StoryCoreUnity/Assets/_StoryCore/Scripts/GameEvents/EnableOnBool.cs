using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.GameEvents {
    public class EnableOnBool : MonoBehaviour {
        [SerializeField] private GameObject m_Target;
        [SerializeField] private GameVariableBool m_Bool;
        [SerializeField] private bool m_EnableWhenFalse;

        private void Reset() {
            m_Target = gameObject;
        }

        private void Start() {
            m_Bool.Changed += OnBoolChanged;
            OnBoolChanged(m_Bool.Value);
        }

        private void OnDestroy() {
            if (m_Bool != null) {
                m_Bool.Changed -= OnBoolChanged;
            }
        }

        private void OnBoolChanged(bool value) {
            m_Target.SetActive(m_EnableWhenFalse ? !value : value);
        }
    }
}