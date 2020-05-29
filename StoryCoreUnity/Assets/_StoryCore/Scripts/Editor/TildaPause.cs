using UnityEditor;

namespace _Scripts.Editor {
    public static class TildaPause {
        [MenuItem("Edit/PauseHotkey _`")]
        private static void PauseEditor() {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
    }
}