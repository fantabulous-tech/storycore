using CoreUtils.Editor;
using UnityEditor;

namespace StoryCore.Choices {
    [CustomEditor(typeof(ChoiceBucket))]
    public class ChoiceKeyBindingsEditor : Editor<ChoiceBucket> {
        private ReorderableListGUI<ChoiceBinding> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<ChoiceBinding>(serializedObject, "m_Bindings");
            m_List.AddColumn("ChoiceHandler", "On this choice handler...");
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