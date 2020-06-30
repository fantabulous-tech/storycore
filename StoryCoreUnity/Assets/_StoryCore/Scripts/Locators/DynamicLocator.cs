using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Locators {
    public class DynamicLocator : MonoBehaviour {

        [SerializeField] private GameVariableVector3 m_LocatorVariable;
        private Vector3 m_StoredPosition;

        private void Update() {
            if (!m_StoredPosition.Approximately(transform.position)) {
                m_StoredPosition = transform.position;
                m_LocatorVariable.Value = m_StoredPosition;
            }
        }

    }
}