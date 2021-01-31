using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore {
    [CustomEditor(typeof(TextReplacementConfig))]
    public class TextReplacementsConfigEditor : Editor<TextReplacementConfig> {
        private ReorderableListGUI<TextReplacementConfig.Replacement> m_List;

        private void OnEnable() {
            m_List = new ReorderableListGUI<TextReplacementConfig.Replacement>(serializedObject, "m_Replacements");
            m_List.AddColumn("Search");
            m_List.AddColumn("Replace");
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            m_List.OnGUI();
        }
    }
}