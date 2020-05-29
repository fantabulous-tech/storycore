using System;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public class MissingTagSearch : AbstractInkTodoSearch {
        protected override void OnHeaderGUI(bool isFirst, bool isLast) {
            using (new GUILayout.HorizontalScope()) {
                m_ShowGUI = EditorGUILayout.Foldout(m_ShowGUI, "Missing Tags" + m_TotalDisplay, true, m_AllExists ? EditorStyles.foldout : ErrorFoldoutStyle);
                GUILayout.FlexibleSpace();
                m_ShowOptions = GUILayout.Toggle(m_ShowOptions, "Options");
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