using CoreUtils.Editor;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore {
    [CustomEditor(typeof(CommandKeyBindings))]
    public class CommandKeyBindingsEditor : Editor<CommandKeyBindings> {
        private ReorderableListGUI<CommandKeyBindings.CommandBinding> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<CommandKeyBindings.CommandBinding>(serializedObject, "m_CommandBindings");
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