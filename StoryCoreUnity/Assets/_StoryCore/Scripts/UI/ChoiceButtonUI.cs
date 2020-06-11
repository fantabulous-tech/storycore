using StoryCore.Utils;
using TMPro;
using UnityEngine;

namespace StoryCore {
    public class ChoiceButtonUI : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_Text;

        private StoryChoice m_Choice;

        public void Fill(StoryChoice choice) {
            m_Choice = choice;
            m_Text.text = choice.DisplayText.ToSpacedName(true, false).Replace(":", ": ");
        }

        public void OnChoice() {
            m_Choice?.Select();
        }
    }
}