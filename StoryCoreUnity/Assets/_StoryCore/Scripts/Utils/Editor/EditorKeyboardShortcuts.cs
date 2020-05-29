using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Utils {
    public static class EditorKeyboardShortcuts {
        private static EditorHotKey[] s_HotKeys;

        [InitializeOnLoadMethod]
        private static void OnLoad() {
            s_HotKeys = new EditorHotKey[0];
            // s_HotKeys = new[] {
            // 	// new EditorHotKey("Pause Toggle", () => EditorApplication.isPaused = !EditorApplication.isPaused, KeyCode.Pause), 
            // 	new EditorHotKey("Stop", () => EditorApplication.isPlaying = false, KeyCode.Escape)
            // };

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            EditorApplication.update += OnUpdate;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnUpdate() {
            s_HotKeys.ForEach(hk => hk.OnEditorUpdate());
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider GetSettingsProvider() {
            return new SettingsProvider("Pause Toggle", SettingsScope.User) {guiHandler = searchContext => { s_HotKeys.ForEach(hk => hk.OnGUI()); }};
        }

        private static void OnPlayModeStateChange(PlayModeStateChange mode) {
            if (mode == PlayModeStateChange.EnteredPlayMode) {
                s_HotKeys.ForEach(hk => hk.OnPlaymode());
            }
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect) {
            OnEditorGUI();
        }

        private static void OnSceneGUI(SceneView sceneView) {
            OnEditorGUI();
        }

        private static void OnEditorGUI() {
            s_HotKeys.ForEach(hk => hk.OnEditorUpdate());
        }
    }
}