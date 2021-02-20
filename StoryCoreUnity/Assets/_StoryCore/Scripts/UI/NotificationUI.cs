using System;
using System.Collections.Generic;
using System.Linq;
using CoreUtils;
using CoreUtils.GameVariables;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoryCore.UI {
    public class NotificationUI : MonoBehaviour, ILocalize {
        [SerializeField] private TextMeshProUGUI m_Title;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField, AutoFillAsset] private TextReplacementConfig m_Replacement;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        private string m_OriginalTitle;
        private string m_OriginalText;
        private string m_TitleTag;
        private string m_TextTag;

        public void Show(string title, string text, float duration, List<string> tags) {
            m_OriginalTitle = title;
            m_OriginalText = text;
            string localizationTag = tags?.FirstOrDefault(t => t.StartsWith("note", StringComparison.OrdinalIgnoreCase));
            m_TitleTag = localizationTag != null ? $"{localizationTag.ToUpper()}_TITLE" : null;
            m_TextTag = localizationTag != null ? $"{localizationTag.ToUpper()}_TEXT" : null;
            Localization.Instance.AddOnLocalizeEvent(this);

            // Automatically close the notification if there's a defined delay, otherwise wait until the next choice occurs.
            if (duration > 0) {
                UnityUtils.DestroyObject(this, duration);
            } else {
                if (m_StoryTellerLocator.Value) {
                    m_StoryTellerLocator.Value.OnChosen += OnChosen;
                }
                SceneManager.sceneLoaded += OnSceneLoad;
            }
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

            SceneManager.sceneLoaded -= OnSceneLoad;
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        public void OnLocalize() {
            if (!m_TitleTag.IsNullOrEmpty() && !m_OriginalTitle.IsNullOrEmpty()) {
                string translation = Localization.Get(m_TitleTag);

                if (Localization.KeyExist(m_TitleTag) && !translation.IsNullOrEmpty()) {
                    m_Title.text = m_Replacement.Convert(translation);
                } else {
                    m_Title.text = m_Replacement.Convert(m_OriginalTitle);
                    Debug.LogWarning($"Could not find translation for '{m_TitleTag}': {m_OriginalTitle}", this);
                }
            } else if (!m_OriginalTitle.IsNullOrEmpty()) {
                Debug.LogWarning($"No localization tag found on notification for: {m_OriginalTitle}", this);
                m_Title.text = m_Replacement.Convert(m_OriginalTitle);
            } else {
                m_Title.text = "";
            }

            if (!m_TextTag.IsNullOrEmpty() && !m_OriginalText.IsNullOrEmpty()) {
                string translation = Localization.Get(m_TextTag);

                if (Localization.KeyExist(m_TextTag) && !translation.IsNullOrEmpty()) {
                    m_Text.text = m_Replacement.Convert(translation);
                } else {
                    m_Text.text = m_Replacement.Convert(m_OriginalText);
                    Debug.LogWarning($"Could not find translation for '{m_TextTag}': {m_OriginalText}", this);
                }
            } else if (!m_OriginalText.IsNullOrEmpty()) {
                Debug.LogWarning($"No localization tag found on notification for: {m_OriginalText}", this);
                m_Text.text = m_Replacement.Convert(m_OriginalText);
            } else {
                m_Text.text = "";
            }
        }
    }
}