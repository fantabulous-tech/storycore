using StoryCore.AssetBuckets;
using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Buckets/Locator Bucket", order = 1)]
    public class LocatorBucket : GenericAssetBucket<GameVariableVector3> { }
}