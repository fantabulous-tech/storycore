using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Choices {
    [CreateAssetMenu(menuName = "GameEvent/Choice Key Bindings")]
    public class ChoiceKeyBindings : BaseBucket {
        [SerializeField, HideInInspector] private ChoiceBinding[] m_Bindings;

        private Dictionary<ChoiceHandler, string> m_HandlerToChoiceText;
        private Dictionary<ChoiceHandler, string> HandlerToChoiceText => UnityUtils.GetOrSet(ref m_HandlerToChoiceText, GetHandlerToText);

        public override string[] ItemNames => m_Bindings.Select(i => i.ChoiceKey).ToArray();
        public ChoiceBinding[] Bindings => m_Bindings;

        public string this[ChoiceHandler handler] => handler ? HandlerToChoiceText?[handler] : null;

        public override bool Has(string itemName) {
            return Bindings.Any(b => b != null && b.ChoiceKey.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        private Dictionary<ChoiceHandler, string> GetHandlerToText() {
            return m_Bindings.ToDictionary(b => b.ChoiceHandler, b => b.ChoiceKey);
        }
    }

    [Serializable]
    public class ChoiceBinding {
        public ChoiceHandler ChoiceHandler;
        public string ChoiceKey;
    }
}