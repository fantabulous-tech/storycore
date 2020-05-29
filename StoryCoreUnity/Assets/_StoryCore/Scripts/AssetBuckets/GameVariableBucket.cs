using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CreateAssetMenu(fileName = "Game Variable Bucket", menuName = "Buckets/Game Variable Bucket")]
    public class GameVariableBucket : GenericAssetBucket<BaseGameVariable> { }
}