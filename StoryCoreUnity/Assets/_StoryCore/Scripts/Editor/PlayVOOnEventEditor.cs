using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.GameEvents {
    [CustomEditor(typeof(PlayVOOnEvent))]
    public class PlayVOOnEventEditor : Editor<PlayVOOnEvent> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Test")) {
                Target.Play();
            }
        }
    }
}