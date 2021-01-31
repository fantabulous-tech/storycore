using CoreUtils.AssetBuckets;
using CoreUtils.Editor;
using StoryCore.Utils;

namespace StoryCore.Commands {
    public class GenericBucketEditor<TBucket, TItem> : Editor<TBucket> where TBucket : GenericBucket<TItem> where TItem : class { }
}