using System;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Choices {
    public class ChoiceManager : Singleton<ChoiceManager> {
        [SerializeField] private ChoiceKeyBindings m_Choices;

        public static ChoiceKeyBindings Choices => Instance ? Instance.m_Choices : null;

        public static event Action<string> ChoiceEvent;

        public override void OnEnable() {
            base.OnEnable();
            Choices.Init();
            Choices.ChoiceEvent.Event += OnChoiceEvent;
        }

        public override void OnDisable() {
            base.OnDisable();
            if (Exists && Choices != null && Choices.ChoiceEvent != null) {
                Choices.ChoiceEvent.Event -= OnChoiceEvent;
            }
        }

        private static void OnChoiceEvent(string choice) {
            ChoiceEvent?.Invoke(choice);
        }
    }
}