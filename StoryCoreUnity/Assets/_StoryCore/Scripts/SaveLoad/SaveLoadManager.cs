using System;
using System.IO;
using CoreUtils;
using CoreUtils.GameVariables;
using Ink.Runtime;
using StoryCore.Commands;
using UnityEngine;

namespace StoryCore.SaveLoad {
    public class SaveLoadManager : Singleton<SaveLoadManager> {
        private const string kSavePathPrefix = "Saves/Profile";
        private const string kBackupPathPrefix = "Saves/Backup/Profile";
        private const string kSceneName = "scene_name";
        private const string kProfileLoadTime = "profile_load_time";
        private const string kProfileTotalTime = "profile_total_time";
        private const string kProfileSaveTime = "profile_save_time";
        private const string kDeviation = "deviation";
        private const string kFullGameDeviation = "full_game";

        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField, AutoFillAsset] private GameVariableInt m_ProfileNum;
        [SerializeField, AutoFillAsset(DefaultName = "CommandLoad")] private CommandHandler m_LoadCommand;
        [SerializeField, AutoFillAsset(DefaultName = "CommandSave")] private CommandHandler m_SaveCommand;

        private float m_LastLoadTime;

        public override void OnEnable() {
            base.OnEnable();

            if (!m_StoryTellerLocator.Value) {
                m_StoryTellerLocator.Changed += SubscribeToStoryTeller;
            } else {
                SubscribeToStoryTeller(m_StoryTellerLocator.Value);
            }

            m_LoadCommand.Event += OnLoad;
            m_SaveCommand.Event += OnSaveEvent;
        }

        public override void OnDisable() {
            base.OnDisable();

            if (m_StoryTellerLocator) {
                m_StoryTellerLocator.Changed -= SubscribeToStoryTeller;
                UnsubscribeFromStoryTeller(m_StoryTellerLocator.Value);
            }

            if (m_LoadCommand != null) {
                m_LoadCommand.Event -= OnLoad;
            }

            if (m_SaveCommand != null) {
                m_SaveCommand.Event -= OnSaveEvent;
            }
        }

        private void SubscribeToStoryTeller(StoryTeller storyTeller) {
            if (storyTeller != null) {
                storyTeller.OnStoryReadyToLoad += OnStoryReadyToLoad;
                storyTeller.OnStoryReady += OnStoryReady;
            }
        }

        private void UnsubscribeFromStoryTeller(StoryTeller storyTeller) {
            if (storyTeller != null) {
                storyTeller.OnStoryReadyToLoad -= OnStoryReadyToLoad;
                storyTeller.OnStoryReady -= OnStoryReady;
            }
        }

        private void OnStoryReadyToLoad() {
            // Make sure the profile # is loaded first.
            SaveLoadVariables.LoadAll();
            LoadProfileData();
        }

        private void LoadProfileData() {
            if (!CanLoadProfile()) {
                return;
            }

            m_LastLoadTime = Time.time;
            string loadPath = GetProfilePath();
            StoryTeller storyTeller = m_StoryTellerLocator.Value;
            Story story = storyTeller.Story;

            if (!File.Exists(loadPath)) {
                StoryDebug.Log($"Load called, but no save to load found at path: {loadPath}. Starting a new story.", this);
                story.variablesState[kProfileLoadTime] = Time.time;

                // Create new save for this game.
                OnSave();
                return;
            }

            string saveJson = File.ReadAllText(loadPath);
            Story testStory = new Story(storyTeller.InkJsonText);

            try {
                // Do not remove.
                // Need to create a test story object in case of a corrupted/invalid save file.
                testStory.state.LoadJson(saveJson);
            }
            catch (Exception e) {
                Debug.LogException(e, this);
                Debug.LogError($"Couldn't load profile because {e.Message}. Starting a new story.", this);
                // try {
                //     File.Delete(loadPath);
                // }
                // catch (Exception e2) {
                //     Debug.LogException(e2);
                // }
                story.variablesState[kProfileLoadTime] = Time.time;
                return;
            }

            story.state.LoadJson(saveJson);
        }

        private void OnStoryReady() {
            if (!CanLoadProfile()) {
                return;
            }

            Story story = m_StoryTellerLocator.Value.Story;
            string deviation = story.variablesState[kDeviation].ToString();

            if (deviation != kFullGameDeviation) {
                StoryDebug.Log($"Not in the '{kFullGameDeviation}' deviation. (deviation = '{deviation}') Skipping profile load.", this);
                return;
            }

            string loadPath = GetProfilePath();

            if (!File.Exists(loadPath)) {
                // Skipping profile time math. Just log the first load time.
                story.variablesState[kProfileLoadTime] = Time.time;
                return;
            }

            int profileNum = m_ProfileNum.Value;
            string storyPath = story.variablesState[kSceneName].ToString();

            string totalTimeString = story.variablesState[kProfileTotalTime].ToString();
            string loadTimeString = story.variablesState[kProfileLoadTime].ToString();
            string saveTimeString = story.variablesState[kProfileSaveTime].ToString();

            if (float.TryParse(totalTimeString, out float totalTime)
                && float.TryParse(loadTimeString, out float loadTime)
                && float.TryParse(saveTimeString, out float saveTime)) {
                // If we have an old save time from a previous session, add it to 'profile_total_time'.
                if (saveTime > 0 && saveTime - loadTime > 0) {
                    story.variablesState[kProfileTotalTime] = totalTime + (saveTime - loadTime);
                    story.variablesState[kProfileSaveTime] = Time.time;
                }
            } else {
                Debug.LogWarning("Could not parse total time from saved story.", this);
            }

            story.variablesState[kProfileLoadTime] = Time.time;
            StoryDebug.Log($"Loading profile #{profileNum} to previous scene {storyPath}. Path: {loadPath}", this);
            story.ChoosePathString("load_game");
        }

        private bool CanLoadProfile() {
            if (StoryCoreSettings.UseDebugInInk) {
                StoryDebug.Log("Using Debug in ink scripts. Skipping load.", this);
                return false;
            }

            Story story = m_StoryTellerLocator.Value.Story;
            string deviation = story.variablesState[kDeviation].ToString();

            if (deviation != kFullGameDeviation) {
                StoryDebug.Log($"Not in the '{kFullGameDeviation}' deviation. (deviation = '{deviation}') Skipping profile load.", this);
                return false;
            }

            return true;
        }

        private void OnSaveEvent(string command) {
            // Don't save right after loading.
            if (Time.time - m_LastLoadTime < 1) {
                StoryDebug.Log($"Just loaded {Time.time - m_LastLoadTime:N2} seconds ago. Skipping save.", this);
                return;
            }

            OnSave();
        }

        private void OnSave() {
            string savePath = GetProfilePath();
            string backupPath = GetBackupPath();
            m_StoryTellerLocator.Value.Story.variablesState[kProfileSaveTime] = Time.time;
            string save = m_StoryTellerLocator.Value.Story.state.ToJson();
            FileUtils.CreateFoldersFor(savePath);
            FileUtils.CreateFoldersFor(backupPath);
            StoryDebug.Log("Save called. Saving to " + savePath, this);
            File.WriteAllText(savePath, save);
            File.WriteAllText(backupPath, save);
        }

        private void OnLoad(string command) {
            // NOTE: Restarting the story will trigger 'LoadProfileData' on 'StoryReady' in StoryTeller.
            m_StoryTellerLocator.Value.RestartStory();
        }

        private string GetProfilePath() {
            return GetProfilePath(m_ProfileNum.Value);
        }

        public static string GetProfilePath(int profileNum) {
            return $"{Application.persistentDataPath}/{kSavePathPrefix}{profileNum}.json";
        }

        private string GetBackupPath() {
            return $"{Application.persistentDataPath}/{kBackupPathPrefix}{m_ProfileNum.Value}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json";
        }
    }
}