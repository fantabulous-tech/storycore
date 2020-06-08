using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class SelectStoryChoiceByNumber : MonoBehaviour {
        [SerializeField] private int m_ChoiceIndex;
        [SerializeField] private StoryTellerLocator m_StoryTeller;

        private StoryTeller StoryTeller => m_StoryTeller ? m_StoryTeller.Value : null;

        public void Choose() {
            if (!StoryTeller) {
                return;
            }

            StoryTeller.Choose(m_ChoiceIndex);
        }
    }
}