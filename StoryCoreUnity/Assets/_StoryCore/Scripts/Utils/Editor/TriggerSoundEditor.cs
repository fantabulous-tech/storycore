using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace Plugins.Utils {
    [CustomEditor(typeof(TriggerSound))]
    public class TriggerSoundEditor : Editor<TriggerSound> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Test")) {
                Target.PlaySound();
            }
        }
    }
}