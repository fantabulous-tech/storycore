using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore {
    [CustomEditor(typeof(ActionBindingBucket))]
    public class ActionBindingBucketEditor : Editor<ActionBindingBucket> {
        private ReorderableListGUI<ActionBindingBucket.ActionBinding> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<ActionBindingBucket.ActionBinding>(serializedObject, "m_ActionBindings");
            m_List.AddColumn("ActionName", 200);
            m_List.AddColumn("GameEvent");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.OnInspectorGUI();
            m_List.OnGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}