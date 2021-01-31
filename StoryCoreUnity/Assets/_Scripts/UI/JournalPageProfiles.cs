using System;
using System.IO;
using CoreUtils;
using Ink.Runtime;
using StoryCore.Commands;
using CoreUtils.GameEvents;
using CoreUtils.GameVariables;
using StoryCore.SaveLoad;
using StoryCore.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryCore.UI {
    public class JournalPageProfiles : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableInt m_LoadedProfile;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField, AutoFillAsset] private GameEvent m_CloseJournal;
        [SerializeField, AutoFillAsset] private CommandHandler m_LoadCommand;
        [SerializeField] private ProfileButton[] m_ProfileButtons;

        [Header("New Profile Info")]
        [SerializeField] private GameObject m_NewProfilePage;
        [SerializeField] private Button m_NewButton;

        [Header("Existing Profile Info")]
        [SerializeField] private GameObject m_ExistingProfilePage;
        [SerializeField] private TextMeshProUGUI m_LabelSaveTime;
        [SerializeField] private TextMeshProUGUI m_LabelPlayTime;
        [SerializeField] private TextMeshProUGUI m_LabelSceneName;
        [SerializeField] private TextMeshProUGUI m_LabelSceneTotal;
        [SerializeField] private TextMeshProUGUI m_LabelBloodstoneCount;
        [SerializeField] private TextMeshProUGUI m_LabelObsidianCount;
        [SerializeField] private Button m_LoadButton;
        [SerializeField] private Button m_ResetButton;
        [SerializeField] private Button m_ResetConfirmButton;

        private int m_SelectedProfileIndex;
        private Story m_Story;

        private Story Story => UnityUtils.GetOrSet(ref m_Story, () => new Story(m_StoryTellerLocator.Value.InkJsonText));

        private void OnEnable() {
            if (!m_StoryTellerLocator.Value) {
                m_StoryTellerLocator.Changed += OnStoryTellerLoaded;
            } else {
                Init();
            }
        }

        private void OnStoryTellerLoaded(StoryTeller storyTeller) {
            if (storyTeller) {
                m_StoryTellerLocator.Changed -= OnStoryTellerLoaded;
                Init();
            }
        }

        private void Init() {
            SelectProfile(m_LoadedProfile.Value - 1);

            m_LoadButton.onClick.AddListener(OnLoadProfile);
            m_ResetButton.onClick.AddListener(OnResetProfile);
            m_ResetConfirmButton.onClick.AddListener(OnResetProfileConfirm);
            m_NewButton.onClick.AddListener(OnLoadProfile);

            for (int i = 0; i < m_ProfileButtons.Length; i++) {
                ProfileButton button = m_ProfileButtons[i];
                button.Init(i);
                button.OnClick += OnProfileButtonClicked;
            }
        }

        private void OnDisable() {
            m_LoadButton.onClick.RemoveListener(OnLoadProfile);
            m_ResetButton.onClick.RemoveListener(OnResetProfile);
            m_ResetConfirmButton.onClick.RemoveListener(OnResetProfileConfirm);
            m_NewButton.onClick.RemoveListener(OnLoadProfile);

            foreach (ProfileButton button in m_ProfileButtons) {
                button.OnClick -= OnProfileButtonClicked;
            }
        }

        private void OnLoadProfile() {
            int profile = m_SelectedProfileIndex + 1;

            // Catch double-clicks causing double-loading.
            if (m_LoadedProfile.Value == profile) {
                return;
            }

            Debug.Log($"Load Profile {m_SelectedProfileIndex + 1} clicked.");
            m_CloseJournal.Raise();
            m_LoadedProfile.Value = m_SelectedProfileIndex + 1;
            m_LoadCommand.Raise();
        }

        private void OnResetProfile() {
            m_ResetButton.gameObject.SetActive(false);
            m_ResetConfirmButton.gameObject.SetActive(true);
        }

        private void OnResetProfileConfirm() {
            m_ResetButton.gameObject.SetActive(true);
            m_ResetConfirmButton.gameObject.SetActive(false);

            int profileNum = m_SelectedProfileIndex + 1;
            string filePath = SaveLoadManager.GetProfilePath(profileNum);

            if (File.Exists(filePath)) {
                Debug.Log("Deleting profile " + filePath, this);
                File.Delete(filePath);
                UpdateDisplay();

                if (profileNum == m_LoadedProfile.Value) {
                    m_StoryTellerLocator.Value.RestartStory();
                }
            } else {
                Debug.LogWarning($"Couldn't delete profile {profileNum}. Doesn't exist at {filePath}.", this); 
            }
        }

        private void UpdateDisplay() {
            for (int i = 0; i < m_ProfileButtons.Length; i++) {
                ProfileButton button = m_ProfileButtons[i];
                button.SetToggle(i == m_SelectedProfileIndex);
                button.UpdateDisplay();
            }

            LoadProfileInfo();
        }

        private void OnProfileButtonClicked(object sender, EventArgs e) {
            int index = m_ProfileButtons.IndexOf(sender);
            if (index >= 0) {
                SelectProfile(index);
            } else {
                Debug.LogError("Couldn't find index of button " + sender);
            }
        }

        private void SelectProfile(int profileIndex) {
            m_SelectedProfileIndex = profileIndex;
            m_ResetButton.gameObject.SetActive(true);
            m_ResetConfirmButton.gameObject.SetActive(false);
            UpdateDisplay();
        }

        private void LoadProfileInfo() {
            int profileNum = m_SelectedProfileIndex + 1;
            string filePath = SaveLoadManager.GetProfilePath(profileNum);
            bool exists = File.Exists(filePath);

            if (exists) {
                string jsonText = File.ReadAllText(filePath);
                try {
                    Story.state.LoadJson(jsonText);
                }
                catch (Exception e) {
                    // Error loading the save. Pretend it doesn't exist. :(
                    Debug.LogException(e, this);
                    exists = false;
                }
            }

            m_ExistingProfilePage.SetActive(exists);
            m_NewProfilePage.SetActive(!exists);

            if (exists) {
                m_LoadButton.gameObject.SetActive(profileNum != m_LoadedProfile.Value);

                DateTime writeTime = File.GetLastWriteTime(filePath);
                m_LabelSaveTime.text = writeTime.ToString("MMM d, h:mm tt");
                m_LabelSceneName.text = Story.variablesState["scene_name"].ToString().ToSpacedName();
                m_LabelBloodstoneCount.text = Story.variablesState["rewards"].ToString();
                m_LabelObsidianCount.text = Story.variablesState["punishments"].ToString();

                string totalTimeString = Story.variablesState["profile_total_time"].ToString();
                string loadTimeString = Story.variablesState["profile_load_time"].ToString();
                string saveTimeString = Story.variablesState["profile_save_time"].ToString();

                if (float.TryParse(totalTimeString, out float totalTime) 
                    && float.TryParse(loadTimeString, out float loadTime) 
                    && float.TryParse(saveTimeString, out float saveTime)) {
                    m_LabelPlayTime.text = TimeSpan.FromSeconds(totalTime + (saveTime - loadTime)).ToString(@"hh\:mm\:ss");
                    m_LabelSceneTotal.text = ((int) Story.variablesState["profile_scene_count"]).ToString("N0");
                } else {
                    Debug.LogWarning("Could not parse total time from saved story.", this);
                }
            }
        }
    }
}