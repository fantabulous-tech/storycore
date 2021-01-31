using System;
using System.IO;
using StoryCore.SaveLoad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryCore.UI {
    public class ProfileButton : MonoBehaviour {
        [SerializeField] private Button m_Button;
        [SerializeField] private TextMeshProUGUI m_ExistingProfileLabel;
        [SerializeField] private TextMeshProUGUI m_NewProfileLabel;

        private int m_ProfileNum;

        private bool ProfileExists => File.Exists(SaveLoadManager.GetProfilePath(m_ProfileNum));

        public event EventHandler OnClick;

        private void OnEnable() {
            m_Button.onClick.AddListener(OnClicked);
        }

        private void OnClicked() {
            OnClick?.Invoke(this, EventArgs.Empty);
        }

        public void Init(int index) {
            m_ProfileNum = index + 1;
            UpdateDisplay();
        }

        public void UpdateDisplay() {
            m_ExistingProfileLabel.gameObject.SetActive(ProfileExists);
            m_NewProfileLabel.gameObject.SetActive(!ProfileExists);
        }

        private void Reset() {
            m_Button = GetComponent<Button>();
        }

        public void SetToggle(bool value) {
            Set(m_Button, value);
        }

        private static void Set(Button button, bool value) {
            SetButtonColors(button, value ? Color.white : Color.black);
        }

        private static void SetButtonColors(Button button, Color color) {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            button.colors = colors;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

            if (label) {
                label.color = color;
            }
        }
    }
}