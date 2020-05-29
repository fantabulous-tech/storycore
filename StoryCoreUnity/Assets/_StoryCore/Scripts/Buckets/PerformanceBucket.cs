using System.IO;
using System.Text.RegularExpressions;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CreateAssetMenu(menuName = "Buckets/Performance Bucket", order = 1)]
    public class PerformanceBucket : GenericAssetBucket<Object> {

#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            if (asset is AnimationClip) {
                string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                path = path.ReplaceRegex(@"\.anim$", ".asset", RegexOptions.IgnoreCase);

                if (File.Exists(path)) {
                    return false;
                }
            }
            return base.EDITOR_CanAdd(asset);
        }
#endif

    }
}