using System.Linq;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.Commands {
    public class OnChoiceAvailableSetActive : MonoBehaviour {
        [SerializeField] private BaseGameEvent[] m_ChoiceEvents;
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
            gameObject.SetActive(m_ChoiceEvents.Any(c => m_StoryTeller.IsValidChoice(c)));
        }
    }
}