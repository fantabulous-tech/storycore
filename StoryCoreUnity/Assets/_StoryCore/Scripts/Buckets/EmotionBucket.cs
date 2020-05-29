using System;
using System.Linq;
using RogoDigital.Lipsync;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CreateAssetMenu(fileName = "Emotion Bucket", menuName = "Buckets/Emotion Bucket")]
    public class EmotionBucket : BaseBucket {
        [SerializeField, AutoFillAsset] private LipSyncProject m_LipSyncProject;

        private string[] m_Emotions;

        public override string[] ItemNames {
            get {
                if (m_Emotions == null || m_Emotions.Length == 0) {
                    m_Emotions = m_LipSyncProject.emotions.ToArray();
                }

                return m_Emotions;
            }
        }

        public override bool Has(string itemName) {
            return ItemNames.Contains(itemName, StringComparer.OrdinalIgnoreCase);
        }
    }
}