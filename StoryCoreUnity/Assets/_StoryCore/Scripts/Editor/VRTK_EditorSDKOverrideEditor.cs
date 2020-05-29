using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using VRTK;

namespace StoryCore.Editor {
    [CustomEditor(typeof(VRTK_EditorSDKOverride))]
    public class VRTK_EditorSDKOverrideEditor : Editor<VRTK_EditorSDKOverride> {
        private string[] m_ListNames;

        private void OnEnable() {
            UpdateOverrideList();
        }

        private void UpdateOverrideList() {
            m_ListNames = Target.Setups.Select(s => s.name).Prepend("None").ToArray();
        }

        public override void OnInspectorGUI() {
            DrawPropertiesExcluding(serializedObject, "m_Index");
            serializedObject.ApplyModifiedProperties();
            GUI.enabled = Target.Manager;
            Target.Index = EditorGUILayout.Popup("SDK Override", Target.Index, m_ListNames);
            if (GUILayout.Button("Refresh")) {
                Target.LoadSDKSetup();
            }
            if (GUI.changed) {
                UpdateOverrideList();
            }
        }
    }
}