using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Utils {
    [InitializeOnLoad]
    public class CompileReplay : ScriptableObject {
        private const string kAutoPlayPref = "CompileReplay_AutoPlay";

        private static CompileReplay s_Instance;
        private static bool s_AutoPlay = true;

        static CompileReplay() {
            EditorApplication.delayCall += CreateSingleton;
        }

        private static void CreateSingleton() {
            if (s_Instance == null) {
                s_Instance = FindObjectOfType<CompileReplay>();
                if (s_Instance == null) {
                    s_Instance = CreateInstance<CompileReplay>();
                }
            }
        }

        public void OnEnable() {
            if (s_Instance && !CheckInstance()) {
                return;
            }

            s_AutoPlay = EditorPrefs.GetBool(kAutoPlayPref, true);
            hideFlags = HideFlags.HideAndDontSave;
            s_Instance = this;
        }

        public CompileReplay() {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange playMode) {
            if (s_AutoPlay && playMode == PlayModeStateChange.EnteredEditMode && EditorApplication.isCompiling) {
                EditorApplication.isPlaying = true;
            }
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider GetSettingsProvider() {
            return new SettingsProvider("CompileReplay", SettingsScope.User) {guiHandler = OnPreferenceGUI};
        }

        private static void OnPreferenceGUI(string context) {
            GUIContent autoPlayContent = new GUIContent("Restore Play After Recompile", "If the editor starts recompiling while in 'Play' mode, then automatically start 'Play' mode again when the recompile completes.");
            bool autoPlay = EditorGUILayout.Toggle(autoPlayContent, s_AutoPlay);
            if (s_AutoPlay != autoPlay) {
                s_AutoPlay = autoPlay;
                EditorPrefs.SetBool(kAutoPlayPref, s_AutoPlay);
            }
        }

        private bool CheckInstance() {
            if (s_Instance != this) {
                if (Application.isPlaying) {
                    Destroy(this);
                } else {
                    DestroyImmediate(this);
                }
                return false;
            }
            return true;
        }
    }
}