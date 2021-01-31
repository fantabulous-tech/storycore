using CoreUtils;
using CoreUtils.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    public class Buckets : Singleton<Buckets> {
        [SerializeField, AutoFillAsset] private CharacterBucket m_CharacterBucket;
        [SerializeField, AutoFillAsset] private PerformanceBucket m_PerformanceBucket;

        public static CharacterBucket Characters => Exists ? UnityUtils.GetOrInstantiate(ref Instance.m_CharacterBucket) : null;
        public static PerformanceBucket Performances => Exists ? UnityUtils.GetOrInstantiate(ref Instance.m_PerformanceBucket) : null;
    }
}