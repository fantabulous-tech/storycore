using StoryCore.Utils;
using UnityEditor;

namespace StoryCore.Editor {
    [CustomEditor(typeof(DistributeChildren))]
    public class DistributeChildrenEditor : Editor<DistributeChildren> {
        private void OnEnable() {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable() {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate() {
            if (Target && !EditorApplication.isPlaying && Target.HasChanged) {
                Target.Distribute();
            }
        }
    }
}