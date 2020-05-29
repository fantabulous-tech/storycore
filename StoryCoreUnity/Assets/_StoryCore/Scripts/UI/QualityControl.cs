using JetBrains.Annotations;
using StoryCore.GameVariables;
using TMPro;
using UnityEngine;

namespace StoryCore.Settings {
    public class QualityControl : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private GameVariableInt m_QualityLevel;

        private void Start() {
            UpdateQualityText();
        }

        private void UpdateQualityText() {
            m_Text.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }

        [UsedImplicitly]
        public void IncreaseQuality() {
            QualitySettings.IncreaseLevel();
            UpdateQualityText();
            if (m_QualityLevel != null) {
                m_QualityLevel.Value = QualitySettings.GetQualityLevel();
            }
        }

        [UsedImplicitly]
        public void DecreaseQuality() {
            QualitySettings.DecreaseLevel();
            UpdateQualityText();
            if (m_QualityLevel != null) {
                m_QualityLevel.Value = QualitySettings.GetQualityLevel();
            }
        }
    }
}