using StoryCore.AssetBuckets;
using StoryCore.Locations;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Buckets/Locator Bucket", order = 1)]
    public class LocationBucket : PrefabInstanceBucket<BaseLocation> { }
}