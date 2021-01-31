using CoreUtils;
using CoreUtils.GameEvents;
using StoryCore.UI;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoryCore {
    public class StoryTellerChoiceUI : MonoBehaviour {
        [SerializeField] private Transform m_ChoiceContainer;
        [SerializeField] private ChoiceButtonUI m_ChoicePrefab;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        private StoryTeller StoryTeller => m_StoryTellerLocator.Value;

        private void OnEnable() {
            UpdateChoices();
            if (StoryTeller != null) {
                StoryTeller.OnChoicesReady += UpdateChoices;
                StoryTeller.OnChoosing += OnChoosing;
                StoryTeller.OnChosen += UpdateChoices;
            }
        }

        private void OnDestroy() {
            if (StoryTeller != null) {
                StoryTeller.OnChoicesReady -= UpdateChoices;
                StoryTeller.OnChoosing -= OnChoosing;
                StoryTeller.OnChosen -= UpdateChoices;
            }
        }

        private void OnChoosing(StoryChoice obj) {
            UpdateChoices();
        }

        private void UpdateChoices() {
            m_ChoiceContainer.DestroyAllChildren();
            if (StoryTeller != null && StoryTeller.HasChoices) {
                StoryTeller.CurrentChoices.ForEach(c => Instantiate(m_ChoicePrefab, m_ChoiceContainer, false).Fill(c));
            }
        }
    }
}