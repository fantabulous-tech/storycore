using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Commands {
    [CustomEditor(typeof(CustomPerformance))]
    public class CustomPerformanceEditor : Editor<CustomPerformance> {
        [SerializeField] private Character m_TestTarget;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying) {
                m_TestTarget = (Character) EditorGUILayout.ObjectField("Target", m_TestTarget, typeof(Character), true);
                GUI.enabled = m_TestTarget;
                if (GUILayout.Button("Test")) {
                    Target.Play(m_TestTarget);
                }
            }
        }
    }
}