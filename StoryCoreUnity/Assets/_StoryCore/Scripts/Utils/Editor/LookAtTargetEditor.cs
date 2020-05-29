using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Editor {
    [CustomEditor(typeof(LookAtTarget)), CanEditMultipleObjects]
    public class LookAtTargetEditor : Editor<LookAtTarget> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Look At Target")) {
                Undo.RecordObjects(Targets.Select(t => t.transform as Object).ToArray(), "Look At Target");
                Targets.ForEach(t => t.UpdateLook());
            }
        }
    }
}