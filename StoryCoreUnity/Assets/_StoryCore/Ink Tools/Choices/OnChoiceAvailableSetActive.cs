using System.Linq;
using StoryCore.Choices;
using UnityEngine;

namespace StoryCore.Commands {
    public class OnChoiceAvailableSetActive : MonoBehaviour {
        [SerializeField] private ChoiceHandler[] m_Choices;
        [SerializeField] private StoryTeller m_StoryTeller;

        private void Start() {
            m_StoryTeller.OnChoicesReady += OnChoicesReady;
            m_StoryTeller.OnChoosing += OnChoicesNotReady;
            gameObject.SetActive(false);
        }

        private void OnChoicesNotReady(StoryChoice choice) {
            gameObject.SetActive(false);
        }

        private void OnChoicesReady() {
            gameObject.SetActive(m_Choices.Any(ChoiceManager.IsValidChoice));
        }
    }
}