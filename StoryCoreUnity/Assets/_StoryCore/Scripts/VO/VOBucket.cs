using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RogoDigital.Lipsync;
using StoryCore.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(fileName = "Prefab Bucket", menuName = "Buckets/VO Bucket", order = 1)]
    public class VOBucket : GenericAssetBucket<LipSyncData> {
        public LipSyncData Get(string prefix, params string[] postFixes) {
            AssetReference[] clips = AssetRefs.Where(i => IsMatch(i.Name, prefix)).ToArray();

            if (clips.Length == 0) {
                Debug.LogWarningFormat(this, "No audio clips found for " + prefix);
                return null;
            }

            foreach (string postFix in postFixes) {
                if (!postFix.IsNullOrEmpty() && clips.Any(c => c.Name.Contains("-" + postFix))) {
                    AssetReference[] matchingClips = clips.Where(c => c.Name.Contains("-" + postFix)).ToArray();
                    return (LipSyncData) matchingClips.GetRandomItem().Asset;
                }
            }

            return (LipSyncData) clips.GetRandomItem().Asset;
        }

        protected override bool HasAsset(AssetReference item, string searchName) {
            return IsMatch(item.Name, searchName);
        }

        public static bool IsMatch(string text, string prefix) {
            return text.Equals(prefix, StringComparison.OrdinalIgnoreCase) || text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && !char.IsLetterOrDigit(text[prefix.Length]);
        }

#if UNITY_EDITOR
        [NonSerialized] private Object m_BaseFolder;
        [NonSerialized] private string m_BaseFolderPath;

        private Object BaseFolder => UnityUtils.GetOrSet(ref m_BaseFolder, () => EDITOR_Sources.FirstOrDefault());
        private string BaseFolderPath => UnityUtils.GetOrSet(ref m_BaseFolderPath, () => BaseFolder ? UnityEditor.AssetDatabase.GetAssetPath(BaseFolder) : null);

        public override string EDITOR_GetAssetName(Object asset) {
            if (!asset) {
                return $"None ({AssetType.Name})";
            }
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            string number = Path.GetFileNameWithoutExtension(path);
            string relativePath = path.Substring(BaseFolderPath.Length);
            string directory = Path.GetDirectoryName(relativePath);
            string prefix = directory.Replace('\\', '/').Trim('/').Replace('/', '.');
            return $"{prefix}.{number}";
        }
#endif
    }
}