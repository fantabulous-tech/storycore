using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    public class Buckets : Singleton<Buckets> {
        [SerializeField] private CharacterBucket m_CharacterBucket;
        [SerializeField] private PerformanceBucket m_PerformanceBucket;
        [SerializeField] private LocationBucket m_LocationBucket;

        public static CharacterBucket Characters => Exists ? Instance.m_CharacterBucket : null;
        public static PerformanceBucket Performances => Exists ? Instance.m_PerformanceBucket : null;
        public static LocationBucket locations => Exists ? Instance.m_LocationBucket : null;
    }
}