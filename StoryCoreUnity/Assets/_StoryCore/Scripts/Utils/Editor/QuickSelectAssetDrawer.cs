using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.Utils.Editor {
    [CustomPropertyDrawer(typeof(QuickSelectAssetAttribute))]
    public class QuickSelectAssetDrawer : PropertyDrawer {
        private QuickSelectAssetAttribute m_Target;
        private QuickSelectAssetAttribute Target => m_Target ?? (m_Target = (QuickSelectAssetAttribute) attribute);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect lineRect = new Rect(position);
            float propHeight = base.GetPropertyHeight(property, label);
            lineRect.height = propHeight;

            using (new EditorGUI.PropertyScope(position, label, property)) {
                EditorGUI.PropertyField(lineRect, property, label);

                if (property.objectReferenceValue) {
                    return;
                }

                lineRect.yMin += propHeight + 2;
                lineRect.xMin += EditorGUIUtility.labelWidth;
                lineRect.height = EditorStyles.popup.lineHeight;

                GUIContent[] labels;
                string[] assetPaths;

                FindAssets(out labels, out assetPaths);

                int exactMatchIndex = labels.IndexOf(l => l.text.Equals(Target.Filter, StringComparison.OrdinalIgnoreCase));

                if (exactMatchIndex > 0) {
                    string exactMatchPath = assetPaths[exactMatchIndex];
                    if (!string.IsNullOrEmpty(exactMatchPath)) {
                        Object exactObj = AssetDatabase.LoadMainAssetAtPath(exactMatchPath);
                        if (exactObj != null) {
                            property.objectReferenceValue = exactObj;
                            return;
                        }
                    }
                }

                int selectedIndex = EditorGUI.Popup(lineRect, GUIContent.none, 0, labels);
                string assetPath = assetPaths[selectedIndex];

                if (!string.IsNullOrEmpty(assetPath)) {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (obj != null) {
                        property.objectReferenceValue = obj;
                    }
                } else {
                    property.objectReferenceValue = null;
                }
            }
        }

        private void FindAssets(out GUIContent[] labels, out string[] assetPaths) {
            string assetFilter = "";

            if (!string.IsNullOrEmpty(Target.Filter)) {
                assetFilter = Target.Filter;
            }
            if (!assetFilter.Contains("t:")) {
                assetFilter += " t:" + fieldInfo.FieldType.Name;
            }

            List<GUIContent> labelsList = new List<GUIContent>();
            List<string> assetPathsList = new List<string>();

            labelsList.Add(new GUIContent("none"));
            assetPathsList.Add("");

            string[] guids = AssetDatabase.FindAssets(assetFilter);

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string prefabName = Path.GetFileNameWithoutExtension(assetPath);

                labelsList.Add(new GUIContent(prefabName, assetPath));
                assetPathsList.Add(assetPath);
            }

            labels = labelsList.ToArray();
            assetPaths = assetPathsList.ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.objectReferenceValue) {
                return base.GetPropertyHeight(property, label);
            }
            return base.GetPropertyHeight(property, label) + 2.0f + EditorStyles.popup.lineHeight + 4.0f;
        }
    }
}