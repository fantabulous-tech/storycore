using System;
using System.Linq;
using CoreUtils;
using CoreUtils.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command List")]
    public class CommandListHandler : CommandHandler {
        [SerializeField, AutoFillAsset] private BucketBucket m_Buckets;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string bucketName = info.Params.FirstOrDefault() ?? "";

            foreach (BaseBucket bucket in m_Buckets.Items) {
                if (bucketName.Equals(bucket.BucketName, StringComparison.OrdinalIgnoreCase)) {
                    bucket.ListOut();
                    break;
                }
            }

            return DelaySequence.Empty;
        }
    }
}