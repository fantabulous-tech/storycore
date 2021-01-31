using System;
using System.Linq;
using CoreUtils;
using JetBrains.Annotations;
using CoreUtils.AssetBuckets;
using CoreUtils.GameEvents;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Buckets/Action Bindings Bucket")]
    public class ActionBindingBucket : BaseBucket {
        [SerializeField, HideInInspector] private ActionBinding[] m_ActionBindings;

        public override string[] ItemNames => m_ActionBindings.Select(i => i.ActionName).ToArray();
        public ActionBinding[] ActionBindings => UnityUtils.GetOrSet(ref m_ActionBindings, () => new ActionBinding[0]);

        public override bool Has(string itemName) {
            return m_ActionBindings.Any(b => b != null && b.ActionName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        [Serializable]
        public class ActionBinding {
            [UsedImplicitly] public string ActionName;
            [UsedImplicitly] public BaseGameEvent GameEvent;
        }
    }
}