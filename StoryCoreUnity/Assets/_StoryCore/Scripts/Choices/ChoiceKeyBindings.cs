using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.Commands;
using JetBrains.Annotations;
using StoryCore.AssetBuckets;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    [CreateAssetMenu(menuName = "GameEvent/Choice Key Bindings")]
    public class ChoiceKeyBindings : BaseBucket {
        [SerializeField, HideInInspector] private ChoiceBinding[] m_Bindings;
        [SerializeField] private GameEventString m_ChoiceEvent;

        private Dictionary<BaseGameEvent, string> m_GameEventToChoiceText;

        public GameEventString ChoiceEvent => m_ChoiceEvent;
        public override string[] ItemNames => m_Bindings.Select(i => i.ChoiceKey).ToArray();

        public void Init() {
            if (!AppTracker.IsPlaying) {
                return;
            }
            m_Bindings.ForEach(BindChoice);
            m_GameEventToChoiceText = m_Bindings.ToDictionary(b => b.GameEvent, b => b.ChoiceKey);
        }

        private void BindChoice(ChoiceBinding binding) {
            if (binding.GameEvent is GameEventString gameEventString) {
                gameEventString.Event += data => Raise(data.IsNullOrEmpty() ? binding.ChoiceKey : $"{binding.ChoiceKey}:{data}");
            } else {
                binding.GameEvent.GenericEvent += () => Raise(binding.ChoiceKey);
            }
        }

        private void Raise(string choiceKey) {
            ChoiceEvent.Raise(choiceKey);
        }

        [Serializable]
        public class ChoiceBinding {
            [UsedImplicitly] public BaseGameEvent GameEvent;
            [UsedImplicitly] public string ChoiceKey;
            [UsedImplicitly] public float Delay;
        }

        public string this[BaseGameEvent gameEvent] => gameEvent ? m_GameEventToChoiceText?[gameEvent] : null;

        public override bool Has(string itemName) {
            return m_Bindings.Any(b => b != null && b.ChoiceKey.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }
    }
}