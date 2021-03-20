using CoreUtils;
using CoreUtils.AssetBuckets;
using StoryCore.Commands;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.SaveLoad {
    public class SaveLoadVariables : Singleton<SaveLoadVariables> {
        [SerializeField, AutoFillAsset] private GameVariableBucket m_GameVariablesToSave;
        [SerializeField, AutoFillAsset] private CommandHandler m_ResetProgressEvent;
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        private StoryTeller StoryTeller => m_StoryTellerLocator.Value;

        public override void OnEnable() {
            base.OnEnable();
            m_ResetProgressEvent.GenericEvent += OnResetProgress;
        }

        private void OnResetProgress() {
            PlayerPrefs.DeleteAll();
            StoryTeller.RestartStory();
        }

        public static void LoadAll() {
            Instance.m_GameVariablesToSave.Items.ForEach(Load);
        }

        public static void SubscribeAll() {
            Instance.m_GameVariablesToSave.Items.ForEach(Subscribe);
        }

        private static void Load(BaseGameVariable variable) {
            if (!variable) {
                return;
            }

            variable.TryInit();

            string value = PlayerPrefs.GetString(variable.name);

            if (!value.IsNullOrEmpty()) {
                // StoryDebug.Log($"Loading {variable} to {value}", variable);
                variable.ValueString = PlayerPrefs.GetString(variable.name);
            }
        }

        private static void Subscribe(BaseGameVariable variable) {
            if (!variable) {
                return;
            }

            variable.GenericEvent += () => Save(variable);
        }

        private static void Save(BaseGameVariable variable) {
            Debug.Log($"Saving {variable} to '{variable.ValueString}'");
            PlayerPrefs.SetString(variable.name, variable.ValueString);
            PlayerPrefs.Save();
        }
    }
}