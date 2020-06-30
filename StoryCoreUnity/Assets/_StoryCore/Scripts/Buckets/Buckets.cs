using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    public class Buckets : Singleton<Buckets> {
        [SerializeField] private CharacterBucket m_CharacterBucket;
        [SerializeField] private PerformanceBucket m_PerformanceBucket;
        [SerializeField] private LocatorBucket m_LocatorBucket;

        public static CharacterBucket Characters => Exists ? Instance.m_CharacterBucket : null;
        public static PerformanceBucket Performances => Exists ? Instance.m_PerformanceBucket : null;
        public static LocatorBucket Locators => Exists ? Instance.m_LocatorBucket : null;
    }
}