using StoryCore.Utils;
using UnityEditor;

namespace StoryCore {
    [CustomEditor(typeof(ChoiceKeyBindings))]
    public class ChoiceKeyBindingsEditor : Editor<ChoiceKeyBindings> {
        private ReorderableListGUI<ChoiceKeyBindings.ChoiceBinding> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<ChoiceKeyBindings.ChoiceBinding>(serializedObject, "m_Bindings");
            m_List.AddColumn("GameEvent", "On this event...");
            m_List.AddColumn("ChoiceKey", "Select this choice.");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.OnInspectorGUI();
            m_List.OnGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}