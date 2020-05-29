using UnityEditor;

namespace StoryCore {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in Globals to be expandable in the Inspector.
    [CustomEditor(typeof(Globals))]
    public class GlobalsEditor : UnityEditor.Editor { }
}