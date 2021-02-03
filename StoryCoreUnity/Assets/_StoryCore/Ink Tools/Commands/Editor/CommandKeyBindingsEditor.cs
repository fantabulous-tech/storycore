using CoreUtils.Editor;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore {
    [CustomEditor(typeof(CommandBucket))]
    public class CommandKeyBindingsEditor : Editor<CommandBucket> {
        private ReorderableListGUI<CommandBucket.CommandBinding> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<CommandBucket.CommandBinding>(serializedObject, "m_CommandBindings");
            m_List.AddColumn("CommandName", "Command", 100);
            m_List.AddColumn("CommandHandler", "Command Handler");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.OnInspectorGUI();
            m_List.OnGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}