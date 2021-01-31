using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreUtils;
using CoreUtils.AssetBuckets;
using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public abstract class AbstractInkTodoSearch {
        [SerializeField] private string m_Name;
        [SerializeField] protected BaseBucket m_Bucket;

        protected int m_ShowItems = -1;
        protected int m_ShowOptions = -1;
        protected string m_CountDisplay;
        protected string m_TotalDisplay;
        private int m_ExistCount;
        protected List<string> m_TodoCollectionKeys = new List<string>();
        protected List<InkTodoCollection> m_TodoCollections = new List<InkTodoCollection>();
        protected bool m_AllExists;

        protected virtual string Name => m_Name;
        private string ShowGUIKey => $"InkTodo-{Name}-ShowGUI";
        private string ShowOptionsKey => $"InkTodo-{Name}-ShowOptions";

        protected bool ShowItems {
            get => EditorUtils.GetEditorPrefBool(ref m_ShowItems, ShowGUIKey);
            set => EditorUtils.SetEditorPrefBool(ref m_ShowItems, value, ShowGUIKey);
        }

        protected bool ShowOptions {
            get => EditorUtils.GetEditorPrefBool(ref m_ShowOptions, ShowOptionsKey);
            set => EditorUtils.SetEditorPrefBool(ref m_ShowOptions, value, ShowOptionsKey);
        }

        public bool DeleteMe { get; private set; }
        public bool MoveUp { get; set; }
        public bool MoveDown { get; set; }

        public BaseBucket Bucket => m_Bucket;

        protected GUIStyle GetCurrentFoldoutStyle() {
            try {
                // If this search is called 'todo', then it's in error by default.
                if (Name.Contains("todo", StringComparison.OrdinalIgnoreCase)) {
                    return ErrorFoldoutStyle;
                }

                // If there is no bucket, there are no keys, or all items exist, then don't use the error style.
                if (!m_Bucket || m_TodoCollectionKeys == null || m_AllExists) {
                    return EditorStyles.foldout;
                }

                // Otherwise, we have errors, so use the error style.
                return ErrorFoldoutStyle;
            }
            catch {
                return null;
            }
        }

        protected virtual void OnHeaderGUI(bool isFirst, bool isLast) {
            using (new GUILayout.HorizontalScope()) {
                ShowItems = EditorGUILayout.Foldout(ShowItems, Name.IsNullOrEmpty() ? "New Search" : Name + m_CountDisplay, true, GetCurrentFoldoutStyle());
                GUILayout.FlexibleSpace();
                ShowOptions = GUILayout.Toggle(ShowOptions, "Options");
                GUI.enabled = !isFirst;
                if (GUILayout.Button("↑")) {
                    MoveUp = true;
                }
                GUI.enabled = !isLast;
                if (GUILayout.Button("↓")) {
                    MoveDown = true;
                }
                GUI.enabled = true;
                if (GUILayout.Button("x")) {
                    DeleteMe = true;
                }
            }
        }

        public void OnGUI(bool missingOnly, bool isFirst, bool isLast, bool fileView) {
            if (missingOnly && m_AllExists) {
                return;
            }

            OnHeaderGUI(isFirst, isLast);

            using (new EditorGUI.IndentLevelScope()) {
                if (ShowOptions) {
                    EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                    OnOptionsGUI();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
                }

                if (!ShowItems) {
                    return;
                }

                foreach (InkTodoCollection collection in m_TodoCollections) {
                    if (collection.Status == InkTodoStatus.Incomplete || !missingOnly) {
                        collection.OnGUI(fileView);
                    }
                }
            }
        }

        protected virtual void OnOptionsGUI() {
            m_Name = EditorGUILayout.TextField("Name", m_Name);
            m_Bucket = EditorGUILayout.ObjectField("Asset Bucket", m_Bucket, typeof(BaseBucket), false) as BaseBucket;
        }

        public void Analyze(InkFileInfo info) {
            if (m_TodoCollectionKeys.Count != m_TodoCollections.Count) {
                m_TodoCollectionKeys.Clear();
                m_TodoCollections.Clear();
            }

            AnalyzeFile(info);
        }

        protected abstract void AnalyzeFile(InkFileInfo inkFileInfo);

        public void FinishAnalysis() {
            m_TodoCollections.Sort((c1, c2) => {
                int result = c1.Status.CompareTo(c2.Status);
                return result != 0 ? result : c1.Key.CompareTo(c2.Key);
            });

            m_TodoCollectionKeys = m_TodoCollections.Select(c => c.Key).ToList();
            m_ExistCount = m_TodoCollections.Count(c => c.Status == InkTodoStatus.Complete);
            m_CountDisplay = " x" + (m_Bucket ? m_ExistCount + "/" : "") + m_TodoCollectionKeys.Count;
            m_TotalDisplay = " x" + m_TodoCollections.Sum(c => c.Count);
            m_AllExists = m_ExistCount == m_TodoCollectionKeys.Count && !Name.Contains("todo", StringComparison.OrdinalIgnoreCase);

            AddUnusedItems();
        }

        protected virtual void AddUnusedItems() {
            if (m_Bucket) {
                foreach (string itemName in m_Bucket.ItemNames.OrderBy(n => n)) {
                    if (itemName != null) {
                        GetOrAddCollection(itemName, InkTodoStatus.Unused);
                    } else {
                        Debug.LogWarning($"Missing item found in {m_Bucket.name}.", m_Bucket);
                    }
                }
            }
        }

        protected InkTodoCollection GetOrAddCollection(string key, InkTodoStatus status = InkTodoStatus.None) {
            return Get(key) ?? Add(key, status);
        }

        protected InkTodoCollection Get(string key) {
            key = key.ToLower();
            int index = m_TodoCollectionKeys.IndexOf(key);
            return index >= 0 ? m_TodoCollections[index] : null;
        }

        protected InkTodoCollection Add(string key, InkTodoStatus status = InkTodoStatus.None) {
            key = key.ToLower();

            if (status == InkTodoStatus.None) {
                // If this is the 'todo' search, then mark them as incomplete.
                if (Name.Contains("todo", StringComparison.OrdinalIgnoreCase)) {
                    status = InkTodoStatus.Incomplete;
                }

                // Else if there is no bucket to find items in, then this is informational, so mark it as complete.
                else if (!m_Bucket) {
                    status = InkTodoStatus.Complete;
                }

                // Else if the bucket has the key, then mark it as complete.
                else if (m_Bucket.Has(key)) {
                    status = InkTodoStatus.Complete;
                }

                // Else the bucket exists and doesn't have the key, so mark it as incomplete.
                else {
                    status = InkTodoStatus.Incomplete;
                }
            }

            InkTodoCollection collection = new InkTodoCollection(Name, key, status);
            m_TodoCollectionKeys.Add(key);
            m_TodoCollections.Add(collection);
            return collection;
        }

        [Serializable]
        protected class InkTodoCollection {
            [SerializeField] private string m_TodoName;
            [SerializeField] private string m_Key;
            [SerializeField] private List<Todo> m_Todos = new List<Todo>();
            [SerializeField] private InkTodoStatus m_Status;

            private string EditorPrefKey => $"InkTodo-{m_TodoName}-{m_Key}";
            
            private int m_Expanded;
            
            private bool Expanded {
                get => EditorUtils.GetEditorPrefBool(ref m_Expanded, EditorPrefKey);
                set => EditorUtils.SetEditorPrefBool(ref m_Expanded, value, EditorPrefKey);
            }

            public string Key => m_Key;
            public InkTodoStatus Status => m_Status;
            public int Count => m_Todos.Count;

            public InkTodoCollection(string name, string key, InkTodoStatus status) {
                m_TodoName = name;
                m_Key = key;
                m_Status = status;
            }

            public void OnGUI(bool fileView) {
                if (fileView) {
                    foreach (Todo todo in m_Todos) {
                        todo.OnGUI();
                    }
                    return;
                }

                if (m_Status == InkTodoStatus.Unused) {
                    EditorGUILayout.LabelField(m_Key, UnusedStyle);
                    return;
                }

                Expanded = EditorGUILayout.Foldout(Expanded, m_Key + (m_Todos.Count == 1 ? "" : " x" + m_Todos.Count), true, m_Status == InkTodoStatus.Complete ? EditorStyles.foldout : ErrorFoldoutStyle);

                if (Expanded) {
                    using (new EditorGUI.IndentLevelScope()) {
                        foreach (Todo todo in m_Todos) {
                            todo.OnGUI();
                        }
                    }
                }
            }

            public void Add(Todo todo) {
                m_Todos.Add(todo);
            }
        }

        [Serializable]
        protected class Todo {
            public InkFileInfo FileInfo;
            public int Line;
            public string Text;

            public Todo(InkFileInfo file, int line, string text) {
                FileInfo = file;
                Line = line;
                Text = text.Trim();
            }

            public void OnGUI() {
                if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), FileInfo.DisplayPath + " (" + Line + "): " + Text, EditorStyles.label)) {
                    AssetDatabase.OpenAsset(FileInfo.InkAsset, Line);
                }
            }
        }

        public virtual void Reset() {
            m_TodoCollectionKeys.Clear();
            m_TodoCollections.Clear();
        }

        private static GUIStyle s_ErrorFoldoutStyle;
        protected static GUIStyle ErrorFoldoutStyle {
            get {
                if (s_ErrorFoldoutStyle == null) {
                    s_ErrorFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                    s_ErrorFoldoutStyle.normal.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.onNormal.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.active.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.focused.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.hover.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.onActive.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.onHover.textColor = new Color(1, 0.3f, 0.3f);
                    s_ErrorFoldoutStyle.onFocused.textColor = new Color(1, 0.3f, 0.3f);
                }

                return s_ErrorFoldoutStyle;
            }
        }

        private static GUIStyle s_UnusedStyle;
        private static GUIStyle UnusedStyle {
            get {
                if (s_UnusedStyle == null) {
                    s_UnusedStyle = new GUIStyle(EditorStyles.label);
                    Color c = EditorStyles.label.normal.textColor;
                    s_UnusedStyle.normal.textColor = new Color(c.r, c.g, c.b, 0.2f);
                    s_UnusedStyle.padding = new RectOffset(15, 0, 0, 0);
                }

                return s_UnusedStyle;
            }
        }

        public void ExportTSV() {
            string path = "Exports/" + Name + ".tsv";
            StringBuilder sb = new StringBuilder("Name\tCount\tExists\n");
            foreach (InkTodoCollection c in m_TodoCollections) {
                sb.AppendLine($"{c.Key}\t{c.Count}\t{c.Status}");
            }
            FileUtils.CreateFoldersFor(path);
            File.WriteAllText(path, sb.ToString());
        }

        protected enum InkTodoStatus {
            None,
            Unused,
            Incomplete,
            Complete
        }
    }
}