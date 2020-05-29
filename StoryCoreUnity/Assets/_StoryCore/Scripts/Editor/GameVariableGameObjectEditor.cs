using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Commands {
    [CustomEditor(typeof(GameVariableGameObject))]
    public class GameVariableGameObjectEditor : Editor<GameVariableGameObject> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying) {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Current", Target.Value, typeof(GameObject), false);
                GUI.enabled = true;
                if (GUILayout.Button("Raise")) {
                    Target.Raise();
                }
            }
        }
    }
}