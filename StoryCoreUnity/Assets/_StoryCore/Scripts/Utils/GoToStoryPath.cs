using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Utils {
    public class GoToStoryPath : MonoBehaviour {
        [SerializeField] private string m_StoryPath;
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;

        public void Go() {
            m_StoryTellerLocator.Value.RestartStory(m_StoryPath);
        }
    }
}