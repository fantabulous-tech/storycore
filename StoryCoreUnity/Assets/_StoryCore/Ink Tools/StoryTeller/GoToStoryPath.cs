using CoreUtils;
using JetBrains.Annotations;
using CoreUtils.GameVariables;
using UnityEngine;

namespace StoryCore.Utils {
    public class GoToStoryPath : MonoBehaviour {
        [SerializeField] private string m_StoryPath;
        [SerializeField] private bool m_RestartStory;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        [UsedImplicitly]
        public void Go() {
            if (m_RestartStory) {
                m_StoryTellerLocator.Value.RestartStory(m_StoryPath);
            } else {
                m_StoryTellerLocator.Value.JumpStory(m_StoryPath);
            }
        }
    }
}