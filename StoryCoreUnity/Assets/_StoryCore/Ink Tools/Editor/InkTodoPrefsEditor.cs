using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [CustomEditor(typeof(InkTodoPrefs))]
    public class InkTodoPrefsEditor : Editor<InkTodoPrefs> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            Target.OnGUI();
        }
    }
}