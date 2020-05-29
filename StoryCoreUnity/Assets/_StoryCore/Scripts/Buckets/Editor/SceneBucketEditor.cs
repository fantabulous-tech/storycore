using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Commands {
    [CustomEditor(typeof(SceneBucket))]
    public class SceneBucketEditor : GenericBucketEditor<SceneBucket, string> {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            GUILayout.Label("Included Scenes:", EditorStyles.boldLabel);

            foreach (string item in Target.Items) {
                GUILayout.Label(item);
            }

            if (GUILayout.Button("Update Build Scenes")) {
                Target.OnValidate();
                EditorUtility.SetDirty(Target);
            }
        }

        [InitializeOnLoadMethod]
        public static void OnLoad() {
            AssetImportTracker.AssetsChanged += OnAssetsChanged;
        }

        private static void OnAssetsChanged(AssetChanges changes) {
            AssetDatabase.FindAssets("t:SceneBucket").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SceneBucket>).ForEach(b => b.OnValidate());
        }
    }
}