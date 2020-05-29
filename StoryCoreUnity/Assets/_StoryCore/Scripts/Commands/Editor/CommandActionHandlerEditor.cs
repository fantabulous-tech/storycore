using UnityEditor;

namespace StoryCore.Commands {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in CommandActionHandler to be expandable in the Inspector.
    [CustomEditor(typeof(CommandActionHandler))]
    public class CommandActionHandlerEditor : UnityEditor.Editor { }
}