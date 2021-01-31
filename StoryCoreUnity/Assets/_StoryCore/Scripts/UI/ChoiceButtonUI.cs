using JetBrains.Annotations;
using Polyglot;
using TMPro;
using UnityEngine;

namespace StoryCore {
    public class ChoiceButtonUI : MonoBehaviour, ILocalize {
        [SerializeField] private TextMeshProUGUI m_Text;

        private StoryChoice m_Choice;

        public void Fill(StoryChoice choice) {
            m_Choice = choice;
            OnLocalize();
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        private void OnDestroy() {
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        [UsedImplicitly]
        public void OnChoice() {
            m_Choice?.Choose();
        }

        public void OnLocalize() {
            m_Text.text = m_Choice.GetLocalizedText();
        }
    }
}