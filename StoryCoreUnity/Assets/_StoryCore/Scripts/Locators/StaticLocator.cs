using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Locators {
    public class StaticLocator : MonoBehaviour {

        [SerializeField] private GameVariableVector3 m_LocatorVariable;

        public void OnEnable() {
            m_LocatorVariable.Value = transform.position;
        }

    }
}