using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameVariables {
    public class SaveLoadVariables : Singleton<SaveLoadVariables> {
        [SerializeField, AutoFillAsset] private GameVariableBucket m_GameVariablesToSave;

        public static BaseGameVariable[] SavedVariables => Instance.m_GameVariablesToSave.Items;

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
                Debug.Log($"Loading {variable} to {value}", variable);
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