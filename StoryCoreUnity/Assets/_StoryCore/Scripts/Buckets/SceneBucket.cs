using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StoryCore.Utils;
using StoryCore.AssetBuckets;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StoryCore.Commands {
    public class SceneBucket : GenericBucket<string> {
        public override string[] ItemNames => m_Items.Select(i => i.ReplaceRegex(@"^.*[/\\]([^/\\]+)\.unity$", "$1", RegexOptions.IgnoreCase)).ToArray();

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private string m_MainPath;
        [SerializeField] private Object m_MainScene;
        [SerializeField] private Object m_SceneFolder;
        [SerializeField] private bool m_AutoUpdateBuildScenes;

        public void OnValidate() {
            ValidateMainScene();
            ValidateSceneList();
            UpdateBuildScenes();
        }

        private void UpdateBuildScenes() {
            if (!m_AutoUpdateBuildScenes) {
                return;
            }
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(Items.Select(path => new EditorBuildSettingsScene(path, true)));
            if (m_MainPath != null) {
                scenes.Insert(0, new EditorBuildSettingsScene(m_MainPath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private void ValidateMainScene() {
            if (!m_MainScene) {
                return;
            }
            string scenePath = AssetDatabase.GetAssetPath(m_MainScene);
            if (!scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) {
                m_MainScene = null;
            } else {
                m_MainPath = scenePath;
            }
        }

        private void ValidateSceneList() {
            if (!m_SceneFolder) {
                return;
            }

            string path = AssetDatabase.GetAssetPath(m_SceneFolder);

            if (!Directory.Exists(path)) {
                Debug.LogWarningFormat(this, m_SceneFolder + " is not a folder.");
                m_SceneFolder = null;
                return;
            }

            m_Items = Directory.GetFiles(path, "*.unity").Select(p => p.Replace("\\", "/").ToLower()).ToArray();
        }

        public override bool Has(string itemName) {
            return Items.Any(item => item != null && (item == itemName.ToLower() || item.EndsWith("/" + itemName.ToLower() + ".unity")));
        }
#endif
    }
}