using JetBrains.Annotations;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using VRTK;

namespace StoryCore.Editor {
    [CustomEditor(typeof(StoryCoreSettings))]
    public class StoryCoreSettingsEditor : Editor<StoryCoreSettings> {
        private SerializedProperty m_ManagerProperty;

        private void OnEnable() {
            if (!Target) {
                return;
            }

            Target.Manager = Target.Manager ? Target.Manager : FindObjectOfType<VRTK_SDKManager>();
            m_ManagerProperty = serializedObject.FindProperty("m_Manager");
        }

        public override void OnInspectorGUI() {
            OnSettingsGUI();

            EditorGUILayout.Space(20);

            DrawPropertiesExcluding(serializedObject, "m_Index", "m_Manager");
            EditorGUILayout.PropertyField(m_ManagerProperty, new GUIContent("VRTK SDK Manager"));
            serializedObject.ApplyModifiedProperties();
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider GetSettingsProvider() {
            return new SettingsProvider("Project/StoryCore", SettingsScope.Project) {guiHandler = searchContext => OnSettingsGUI()};
        }

        private static void OnSettingsGUI() {
            EditorGUILayout.HelpBox("NOTE: These settings only apply at editor time.", MessageType.Info);
            StoryCoreSettings.UseDebugInInk = EditorGUILayout.Toggle("Set 'isDebug' in Ink", StoryCoreSettings.UseDebugInInk);
            StoryCoreSettings.ForceSimulationMode = EditorGUILayout.Toggle("Force VR Simulator Mode", StoryCoreSettings.ForceSimulationMode);
            StoryCoreSettings.EnableLogging = EditorGUILayout.Toggle("Enable StoryDebug.Log()", StoryCoreSettings.EnableLogging);
        }
    }
}