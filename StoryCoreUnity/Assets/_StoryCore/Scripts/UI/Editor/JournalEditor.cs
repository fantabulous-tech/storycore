using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.UI {
    [CustomEditor(typeof(Journal))]
    public class JournalEditor : Editor<Journal> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            foreach (Journal.PageSet page in Target.Pages) {
                if (GUILayout.Button($"Show {page.PageState}")) {
                    Target.ShowPage(page.PageState);
                }
            }
        }
    }
}