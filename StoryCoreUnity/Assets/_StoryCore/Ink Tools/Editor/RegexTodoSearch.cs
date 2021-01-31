using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreUtils;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public class RegexTodoSearch : AbstractInkTodoSearch {
        [SerializeField] private string m_Search;
        [SerializeField] private string m_Result;
        [SerializeField] private List<string> m_Exclusions = new List<string>();

        private string m_LastSearch;
        private Regex m_SearchRegex;
        
        protected override void OnOptionsGUI() {
            base.OnOptionsGUI();
            m_Search = EditorGUILayout.TextField("Search Regex", m_Search);
            m_Result = EditorGUILayout.TextField("Result Regex", m_Result);

            EditorGUILayout.PrefixLabel("Exclusions:", EditorStyles.boldLabel);
            
            int removeIndex = -1;
            for (int i = 0; i < m_Exclusions.Count; i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                m_Exclusions[i] = GUILayout.TextField(m_Exclusions[i]);

                if (GUILayout.Button("x", GUILayout.Width(20))) {
                    removeIndex = i;
                }

                GUILayout.EndHorizontal();
            }

            if (removeIndex >= 0) {
                m_Exclusions.RemoveAt(removeIndex);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);

            if (GUILayout.Button("Add", GUILayout.Width(50))) {
                m_Exclusions.Add("");
            }

            GUILayout.EndHorizontal();
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

                        if (m_Exclusions.Contains(result, StringComparison.OrdinalIgnoreCase)) {
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