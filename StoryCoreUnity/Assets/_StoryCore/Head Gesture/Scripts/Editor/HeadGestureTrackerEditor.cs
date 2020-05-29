using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.HeadGesture {
    [CustomEditor(typeof(HeadGestureTracker))]
    public class HeadGestureTrackerEditor : Editor<HeadGestureTracker> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Center Poses")) {
                Target.ResetNod();
            }
        }
    }
}