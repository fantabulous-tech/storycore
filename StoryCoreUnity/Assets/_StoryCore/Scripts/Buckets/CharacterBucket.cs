using System;
using System.Linq;
using StoryCore.Characters;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    [CreateAssetMenu(menuName = "Buckets/Character Bucket", order = 1)]
    public class CharacterBucket : PrefabInstanceBucket<BaseCharacter> {
        protected override string GetName(BaseCharacter character) {
            return character.Name;
        }

        public override BaseCharacter Get(string assetName) {
            if (assetName.IsNullOrEmpty()) {
                return null;
            }
            BaseCharacter character = base.Get(assetName);

            if (character == null) {
                character = Instances.Values.FirstOrDefault(c => c.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
            }

            return character;
        }
    }
}