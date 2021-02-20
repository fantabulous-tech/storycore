using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Editor {
    [CustomEditor(typeof(CharacterTester))]
    public class CharacterTesterEditor : Editor<CharacterTester> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (!Application.isPlaying) {
                return;
            }

            Target.Index = EditorGUILayout.IntSlider("Index", Target.Index, 0, Target.Max);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev")) {
                Target.PreviousPerformance();
            }
            if (GUILayout.Button("Next")) {
                Target.NextPerformance();
            }
            GUILayout.EndHorizontal();
        }
    }
}