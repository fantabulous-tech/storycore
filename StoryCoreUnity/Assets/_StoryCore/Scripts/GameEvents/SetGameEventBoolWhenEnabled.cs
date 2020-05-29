using UnityEngine;

namespace StoryCore.GameVariables {
    public class SetGameEventBoolWhenEnabled : MonoBehaviour {
        [SerializeField] private GameVariableBool m_Bool;
        [SerializeField] private bool m_Value;

        private void OnEnable() {
            m_Bool.Value = m_Value;
        }
    }
}