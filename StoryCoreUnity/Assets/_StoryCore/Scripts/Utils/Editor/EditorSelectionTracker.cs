using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace StoryCore.Utils {
    public static class EditorSelectionTracker {
        private static Object s_LastSelection;
        private static int s_LastSelectionCount;

        public static event Action<Object> SelectionChanged;
        public static event Action<Object[]> SelectedObjectsChanged;

        static EditorSelectionTracker() {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update() {
            if (s_LastSelection == Selection.activeObject && s_LastSelectionCount == Selection.objects.Length) {
                return;
            }

            s_LastSelection = Selection.activeObject;
            s_LastSelectionCount = Selection.objects.Length;

            if (SelectionChanged != null) {
                SelectionChanged(Selection.activeObject);
            }

            if (SelectedObjectsChanged != null) {
                SelectedObjectsChanged(Selection.objects);
            }
        }
    }
}