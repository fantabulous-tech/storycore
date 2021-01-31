using System;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public class MissingTagSearch : AbstractInkTodoSearch {
        protected override string Name => "MissingTag";

        protected override void OnHeaderGUI(bool isFirst, bool isLast) {
            using (new GUILayout.HorizontalScope()) {
                ShowItems = EditorGUILayout.Foldout(ShowItems, "Missing Tags" + m_TotalDisplay, true, m_AllExists ? EditorStyles.foldout : ErrorFoldoutStyle);
                GUILayout.FlexibleSpace();
                ShowOptions = GUILayout.Toggle(ShowOptions, "Options");
            }
        }

        protected override void OnOptionsGUI() {
            GUILayout.Label("No options.");
        }

        protected override void AnalyzeFile(InkFileInfo info) {
            for (int i = 0; i < info.Lines.Length; i++) {
                string line = info.Lines[i];
                if (InkEditorUtils.CanTag(line)) {
                    InkTodoCollection collection = GetOrAddCollection(info.DisplayPath, InkTodoStatus.Incomplete);
                    collection.Add(new Todo(info, i + 1, line));
                }
            }
        }
    }
}