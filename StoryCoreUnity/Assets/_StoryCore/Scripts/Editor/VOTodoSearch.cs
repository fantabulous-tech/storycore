using System;
using System.Linq;
using System.Text.RegularExpressions;
using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.InkTodo {
    [Serializable]
    public class VOTodoSearch : AbstractInkTodoSearch {
        protected override void OnHeaderGUI(bool isFirst, bool isLast) {
            using (new GUILayout.HorizontalScope()) {
                m_ShowGUI = EditorGUILayout.Foldout(m_ShowGUI, "VO" + m_CountDisplay, true, GetCurrentFoldoutStyle());
                GUILayout.FlexibleSpace();
                m_ShowOptions = GUILayout.Toggle(m_ShowOptions, "Options");
            }
        }

        protected override void OnOptionsGUI() {
            m_Bucket = EditorGUILayout.ObjectField("VO Bucket", m_Bucket, typeof(BaseBucket), false) as BaseBucket;
        }

        protected override void AnalyzeFile(InkFileInfo info) {
            string currentKnot = null;

            for (int i = 0; i < info.Lines.Length; i++) {
                string line = info.Lines[i];

                Match knotMatch = InkEditorUtils.KnotRegex.Match(line);

                if (knotMatch.Success) {
                    currentKnot = knotMatch.Groups["knot"].Value;
                    continue;
                }

                if (currentKnot.IsNullOrEmpty() || !line.Contains('#') || line.Trim().StartsWith("//")) {
                    continue;
                }

                MatchCollection tagMatches = InkEditorUtils.TagRegex.Matches(line);

                for (int j = 0; j < tagMatches.Count; j++) {
                    Match match = tagMatches[j];
                    if (int.TryParse(match.Groups["tag"].Value, out int tagId)) {
                        string result = currentKnot + "." + tagId.ToString("00");
                        InkTodoCollection collection = GetOrAddCollection(result);
                        collection.Add(new Todo(info, i + 1, line));
                    }
                }
            }
        }

        protected override void AddUnusedItems() {
            if (m_Bucket) {
                foreach (string itemName in m_Bucket.ItemNames.OrderBy(n => n)) {
                    if (itemName != null) {
                        InkTodoCollection collection = Get(itemName);

                        if (collection != null) {
                            continue;
                        }

                        if (!m_TodoCollectionKeys.Any(k => VOBucket.IsMatch(itemName, k))) {
                            GetOrAddCollection(itemName, InkTodoStatus.Unused);
                        }
                    } else {
                        Debug.LogWarning($"Missing item found in {m_Bucket.name}.", m_Bucket);
                    }
                }
            }
        }
    }
}