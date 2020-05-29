using System;
using System.Text.RegularExpressions;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public class RegexTodoSearch : AbstractInkTodoSearch {
        [SerializeField] private string m_Search;
        [SerializeField] private string m_Result;

        private string m_LastSearch;
        private Regex m_SearchRegex;

        protected override void OnOptionsGUI() {
            base.OnOptionsGUI();
            m_Search = EditorGUILayout.TextField("Search Regex", m_Search);
            m_Result = EditorGUILayout.TextField("Result Regex", m_Result);
        }

        protected override void AnalyzeFile(InkFileInfo info) {
            if (m_Search.IsNullOrEmpty()) {
                return;
            }

            CheckChanges();

            for (int i = 0; i < info.Lines.Length; i++) {
                string line = info.Lines[i];
                if (m_SearchRegex.IsMatch(line)) {
                    string[] results = m_SearchRegex.Replace(line, m_Result.IsNullOrEmpty() ? "$0" : m_Result).Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string result in results) {
                        if (result == null) {
                            Debug.LogWarning($"Missing item found in {m_Bucket.name}.", m_Bucket);
                            continue;
                        }
                        InkTodoCollection collection = GetOrAddCollection(result);
                        collection.Add(new Todo(info, i + 1, line));
                    }
                }
            }
        }

        public override void Reset() {
            base.Reset();
            m_SearchRegex = m_Search.IsNullOrEmpty() ? null : new Regex(m_Search);
        }

        private void CheckChanges() {
            if (m_Search != m_LastSearch || m_SearchRegex == null) {
                m_LastSearch = m_Search;
                m_SearchRegex = m_Search.IsNullOrEmpty() ? null : new Regex(m_Search);
            }
        }
    }
}