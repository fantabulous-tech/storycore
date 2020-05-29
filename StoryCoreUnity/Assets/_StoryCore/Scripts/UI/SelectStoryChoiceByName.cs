using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class SelectStoryChoiceByName : MonoBehaviour {
        [SerializeField] private string m_ChoiceName;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTeller;

        private StoryTeller StoryTeller => m_StoryTeller ? m_StoryTeller.Value : null;

        public void Choose() {
            if (!StoryTeller) {
                return;
            }

            if (!m_ChoiceName.IsNullOrEmpty()) {
                if (StoryTeller.CanChoose(m_ChoiceName)) {
                    StoryTeller.Choose(m_ChoiceName);
                } else {
                    Debug.LogError($"Couldn't choose {m_ChoiceName}.", this);
                }
            }
        }
    }
}