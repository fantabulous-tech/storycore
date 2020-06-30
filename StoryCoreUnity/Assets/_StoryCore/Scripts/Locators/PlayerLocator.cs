using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Locators {
    public class PlayerLocator : MonoBehaviour {

        [SerializeField] private GameVariableVector3 m_PlayerPositionVariable;
        [SerializeField] private GameVariableVector3 m_InFrontOfPlayerVariable;

        private void Update() {
            m_PlayerPositionVariable.Value = transform.position;
            m_InFrontOfPlayerVariable.Value = transform.position + transform.forward*2.0f;
        }

    }
}