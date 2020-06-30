using System;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class ChoiceManager : Singleton<ChoiceManager> {
        [SerializeField] private ChoiceKeyBindings m_Choices;

        public static ChoiceKeyBindings Choices => Instance ? Instance.m_Choices : null;

        public static event Action<string> ChoiceEvent;

        protected override void OnEnable() {
            base.OnEnable();
            Choices.Init();
            Choices.ChoiceEvent.Event += OnChoiceEvent;
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (Choices != null && Choices.ChoiceEvent != null) {
                Choices.ChoiceEvent.Event -= OnChoiceEvent;
            }
        }

        private static void OnChoiceEvent(string choice) {
            ChoiceEvent?.Invoke(choice);
        }
    }
}