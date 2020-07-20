using System.Collections.Generic;
using StoryCore.Characters;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CustomEditor(typeof(CharacterBucket))]
    public class CharacterBucketEditor : Editor<CharacterBucket> {
        private UnityEditor.Editor m_ParentEditor;
        private UnityEditor.Editor ParentEditor => UnityUtils.GetOrSet(ref m_ParentEditor, GetEditor);

        private UnityEditor.Editor GetEditor() {
            return CreateEditor(Target, typeof(AssetBucketEditor));
        }

        public override void OnInspectorGUI() {
            ParentEditor.OnInspectorGUI();

            if (!Application.isPlaying) {
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Instances:", EditorStyles.boldLabel);
            foreach (KeyValuePair<string, BaseCharacter> keyValuePair in Target.Instances) {
                EditorGUILayout.ObjectField(keyValuePair.Key, keyValuePair.Value, typeof(BaseCharacter), true);
            }
        }
    }
}