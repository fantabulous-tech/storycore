using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    public class InkTodoWindow : EditorWindow {
        private const string kWindowName = "Ink Todo";
        private const string kInkTodoFilePref = "InkTodo.FilePath";

        private static InkTodoWindow s_Instance;

        private static InkTodoWindow Instance => UnityUtils.GetOrSet(ref s_Instance, () => GetWindow<InkTodoWindow>(kWindowName));

        [MenuItem("Window/Ink Todo List", false, 2301)]
        public static void OpenWindow() {
            Instance.Show();
        }

        private InkTodoPrefs m_Prefs;
        // private bool m_

        private void OnEnable() {
            if (!m_Prefs) {
                string path = EditorPrefs.GetString(kInkTodoFilePref, null);

                if (path != null) {
                    DefaultAsset inkFile = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
                    m_Prefs = InkTodoPrefs.Load(inkFile);
                }
            }

            if (m_Prefs) {
                m_Prefs.Analyze();
            }
        }

        private void OnGUI() {
            if (!m_Prefs) {
                Object newInkFile = EditorGUILayout.ObjectField("Ink File", null, typeof(DefaultAsset), false) as DefaultAsset;

                if (newInkFile == null) {
                    return;
                }

                m_Prefs = InkTodoPrefs.Load(newInkFile);
                EditorPrefs.SetString(kInkTodoFilePref, AssetDatabase.GetAssetPath(newInkFile));
            }

            m_Prefs = EditorGUILayout.ObjectField("Ink Todo Prefs File", m_Prefs, typeof(InkTodoPrefs), false) as InkTodoPrefs;

            if (m_Prefs == null) {
                return;
            }

            SerializedObject so = new SerializedObject(m_Prefs);
            // EditorGUILayout.PropertyField(so.FindProperty("m_InkFileAssets"), true);

            m_Prefs.OnGUI();

            if (GUI.changed) {
                EditorUtility.SetDirty(m_Prefs);
            }

            so.ApplyModifiedProperties();
        }
    }
}