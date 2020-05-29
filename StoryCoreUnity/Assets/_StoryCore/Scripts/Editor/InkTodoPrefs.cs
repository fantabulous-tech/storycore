using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.InkTodo {
    public class InkTodoPrefs : ScriptableObject {
        [SerializeField] private DefaultAsset m_RootInkFile;
        [SerializeField, HideInInspector] private VOTodoSearch m_VOSearch;
        [SerializeField, HideInInspector] private MissingTagSearch m_MissingTagSearch;
        [SerializeField, HideInInspector] private List<RegexTodoSearch> m_Searches = new List<RegexTodoSearch>();
        [SerializeField, HideInInspector] private bool m_MissingOnly;
        [SerializeField, HideInInspector] private bool m_ShowSummaryByFile;
        [SerializeField, HideInInspector] private bool m_ShowExcludes;
        [SerializeField, HideInInspector] private List<string> m_Excluded;

        private List<InkFileInfo> m_InkFiles = new List<InkFileInfo>();
        private Vector2 m_ScrollPos;
        private Vector2 m_InclusionScrollPos;

        public void Analyze() {
            if (m_RootInkFile == null) {
                Debug.LogWarningFormat(this, "No ink file found.");
                return;
            }

            string rootInkFilePath = AssetDatabase.GetAssetPath(m_RootInkFile);

            m_InkFiles = InkUtils.GetInkFiles(rootInkFilePath);
            m_VOSearch.Reset();
            m_MissingTagSearch.Reset();
            m_Searches.ForEach(s => s.Reset());

            foreach (InkFileInfo info in m_InkFiles) {
                if (m_Excluded.Contains(info.Path, StringComparer.OrdinalIgnoreCase)) {
                    info.Excluded = true;
                    continue;
                }

                m_VOSearch.Analyze(info);
                m_MissingTagSearch.Analyze(info);
                foreach (RegexTodoSearch search in m_Searches) {
                    search.Analyze(info);
                }
            }

            m_VOSearch.FinishAnalysis();
            m_MissingTagSearch.FinishAnalysis();
            foreach (RegexTodoSearch search in m_Searches) {
                search.FinishAnalysis();
            }
        }

        public void OnGUI() {
            m_MissingOnly = EditorGUILayout.Toggle("Show Todos Only", m_MissingOnly);
            m_ShowSummaryByFile = EditorGUILayout.Toggle("Show Summary By File", m_ShowSummaryByFile);

            m_ShowExcludes = EditorGUILayout.Foldout(m_ShowExcludes, "Files to Analyze");

            if (m_ShowExcludes) {
                m_InclusionScrollPos = EditorGUILayout.BeginScrollView(m_InclusionScrollPos, GUILayout.Height(400));

                foreach (InkFileInfo inkFile in m_InkFiles) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    bool included = GUILayout.Toggle(!inkFile.Excluded, inkFile.DisplayPath);

                    if (included == inkFile.Excluded) {
                        inkFile.Excluded = !included;

                        if (!included) {
                            m_Excluded.Add(inkFile.Path);
                        } else {
                            m_Excluded.Remove(inkFile.Path);
                        }

                        EditorUtility.SetDirty(this);
                    }

                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Analyze")) {
                    Analyze();
                }
                if (GUILayout.Button("Export")) {
                    Export();
                }
            }

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            m_VOSearch.OnGUI(m_VOSearch.Bucket && m_MissingOnly, true, false, m_ShowSummaryByFile);
            m_MissingTagSearch.OnGUI(m_MissingOnly, true, false, m_ShowSummaryByFile);

            RegexTodoSearch deleteMe = null;
            RegexTodoSearch moveUp = null;
            RegexTodoSearch moveDown = null;

            for (int i = 0; i < m_Searches.Count; i++) {
                RegexTodoSearch search = m_Searches[i];
                search.OnGUI(m_MissingOnly, i == 0, i == m_Searches.Count - 1, m_ShowSummaryByFile);
                if (search.DeleteMe) {
                    deleteMe = search;
                }
                if (search.MoveUp) {
                    moveUp = search;
                }
                if (search.MoveDown) {
                    moveDown = search;
                }
            }

            if (deleteMe != null) {
                m_Searches.Remove(deleteMe);
            } else if (moveUp != null) {
                m_Searches.MoveUp(moveUp);
                moveUp.MoveUp = false;
            } else if (moveDown != null) {
                m_Searches.MoveDown(moveDown);
                moveDown.MoveDown = false;
            }

            using (new GUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+")) {
                    m_Searches.Add(new RegexTodoSearch());
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Export() {
            m_VOSearch.ExportTSV();
            m_MissingTagSearch.ExportTSV();
            foreach (RegexTodoSearch search in m_Searches) {
                search.ExportTSV();
            }
        }

        public static InkTodoPrefs Load(Object inkFile) {
            if (inkFile == null) {
                Debug.LogWarning("Cannot load InkTodoPrefs. No ink file found.");
                return null;
            }

            string inkPath = AssetDatabase.GetAssetPath(inkFile);
            string directory = Path.GetDirectoryName(inkPath);
            string prefsName = Path.GetFileNameWithoutExtension(inkPath) + ".asset";
            string prefsPath = Path.Combine(directory, prefsName);

            InkTodoPrefs prefs = AssetDatabase.LoadAssetAtPath<InkTodoPrefs>(prefsPath);

            if (!prefs) {
                prefs = CreateInstance<InkTodoPrefs>();
                AssetDatabase.CreateAsset(prefs, prefsPath);
            }

            prefs.Analyze();

            return prefs;
        }
    }
}