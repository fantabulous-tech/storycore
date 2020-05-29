using UnityEditor;

namespace StoryCore {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in CommandManager to be expandable in the Inspector.
    [CustomEditor(typeof(CommandManager))]
    public class CommandManagerEditor : UnityEditor.Editor { }
}