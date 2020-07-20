using System.Linq;
using StoryCore.GameEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace StoryCore.Choices {
    public class OnChoiceAvailableSetActive : MonoBehaviour {
        [FormerlySerializedAs("m_ChoiceEvents"),SerializeField] private ChoiceHandler[] m_ChoiceHandlers;
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
            gameObject.SetActive(m_ChoiceHandlers.Any(ChoiceManager.IsValidChoice));
        }
    }
}