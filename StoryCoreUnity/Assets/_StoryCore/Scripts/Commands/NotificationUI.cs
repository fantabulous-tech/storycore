using StoryCore.GameVariables;
using StoryCore.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoryCore.Commands {
    public class NotificationUI : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_Title;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private TextReplacementConfig m_Replacement;
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;

        public void Show(ScriptCommandInfo info) {
            m_Title.text = info.NamedParams.ContainsKey("title") ? info.NamedParams["title"] : "";

            if (info.NamedParams.ContainsKey("text")) {
                m_Text.text = info.NamedParams["text"];
            } else {
                m_Text.text = info.NamedParams.Count == 0 ? info.Params.AggregateToString(" ") : "";
            }

            m_Text.text = m_Replacement.Convert(m_Text.text);

            float delay = info.NamedParams.ContainsKey("delay") && float.TryParse(info.NamedParams["delay"], out float d) ? d : -1;

            // Automatically close the notification if there's a defined delay, otherwise wait until the next choice occurs.
            if (delay > 0) {
                UnityUtils.DestroyObject(this, delay);
            }

            m_StoryTellerLocator.Value.OnChosen += OnChosen;
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1) {
            UnityUtils.DestroyObject(this);
        }

        private void OnChosen() {
            UnityUtils.DestroyObject(this);
        }

        private void OnDestroy() {
            if (m_StoryTellerLocator.Value) {
                m_StoryTellerLocator.Value.OnChosen -= OnChosen;
            }
        }
    }
}