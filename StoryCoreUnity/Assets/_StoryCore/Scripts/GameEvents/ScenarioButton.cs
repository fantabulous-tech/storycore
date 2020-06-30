using StoryCore.GameVariables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bindings {
    public class ScenarioButton : MonoBehaviour {
        [SerializeField] private Button m_Button;
        [SerializeField] private TextMeshProUGUI m_Label;
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;

        private bool m_LastValue;

        public string ScenarioPath { get; set; }

        private void Start() {
            m_Button.onClick.AddListener(OnClick);
        }

        private void OnDestroy() {
            m_Button.onClick.RemoveListener(OnClick);
        }

        private void Reset() {
            m_Button = GetComponentInChildren<Button>();
            m_Label = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetText(string value) {
            m_Label.text = value;
        }

        private void OnClick() {
            m_StoryTellerLocator.Value.RestartStory(ScenarioPath);
        }
    }
}