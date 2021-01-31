using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore.Commands {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in ProviderReferences to be expandable in the Inspector.
    [CustomEditor(typeof(Buckets))]
    public class BucketsEditor : Editor<Buckets> { }
}