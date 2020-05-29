using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CustomEditor(typeof(EmotionBucket))]
    public class EmotionBucketEditor : Editor<EmotionBucket> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            GUILayout.Label("Emotions", EditorStyles.boldLabel);

            foreach (string itemName in Target.ItemNames.OrderBy(n => n)) {
                GUILayout.Label(itemName);
            }
        }
    }
}