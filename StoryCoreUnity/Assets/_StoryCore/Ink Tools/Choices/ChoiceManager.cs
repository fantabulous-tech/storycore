using System;
using System.Collections.Generic;
using System.Linq;
using CoreUtils;
using UnityEngine;

namespace StoryCore.Choices {
    public class ChoiceManager : Singleton<ChoiceManager> {
        [SerializeField, AutoFillFromScene] private StoryTeller m_StoryTeller;
        [SerializeField, AutoFillAsset] private ChoiceBucket m_Choices;

        public static event EventHandler AllChoicesReady; 

        private readonly Dictionary<StoryChoice, ChoiceHandler> m_CurrentChoiceHandlers = new Dictionary<StoryChoice, ChoiceHandler>();

        private ChoiceBinding[] Bindings => m_Choices.Bindings;
        private StoryTeller StoryTeller => m_StoryTeller;

        public override void OnEnable() {
            base.OnEnable();
            if (!AppTracker.IsPlaying) {
                return;
            }

            SubscribeToStoryTeller();
            SubscribeToChoiceHandlers();

            foreach (ChoiceBinding binding in Bindings) {
                binding.ChoiceHandler.TryInit();
            }
        }

        private void OnDestroy() {
            UnsubscribeToStoryTeller();
            UnsubscribeToChoiceHandlers();
        }

        private void SubscribeToStoryTeller() {
            StoryTeller.OnChoicesReady += OnChoicesReady;
            StoryTeller.OnChoicesWaiting += OnChoicesWaiting;
            StoryTeller.OnChoosing += OnChoosing;
            StoryTeller.OnChosen += OnChosen;
        }

        private void UnsubscribeToStoryTeller() {
            if (StoryTeller == null) {
                return;
            }

            StoryTeller.OnChoicesReady -= OnChoicesReady;
            StoryTeller.OnChoicesWaiting -= OnChoicesWaiting;
            StoryTeller.OnChoosing -= OnChoosing;
            StoryTeller.OnChosen -= OnChosen;
        }

        private void SubscribeToChoiceHandlers() {
            Bindings.ForEach(SubscribeToChoiceHandler);
        }

        private void UnsubscribeToChoiceHandlers() {
            Bindings?.ForEach(UnsubscribeToChoiceHandler);
        }

        private void SubscribeToChoiceHandler(ChoiceBinding binding) {
            ChoiceHandler handler = binding.ChoiceHandler;
            handler.ChoiceEvaluating += OnChoiceEvaluating;
            handler.ChoiceEvaluationFailed += OnChoiceEvaluationFailed;
        }

        private void UnsubscribeToChoiceHandler(ChoiceBinding binding) {
            if (binding == null || !binding.ChoiceHandler) {
                return;
            }

            ChoiceHandler handler = binding.ChoiceHandler;
            handler.ChoiceEvaluating += OnChoiceEvaluating;
            handler.ChoiceEvaluationFailed += OnChoiceEvaluationFailed;
        }

        private void OnChoicesReady() {
            m_CurrentChoiceHandlers.Clear();
            StoryTeller.CurrentChoices.ForEach(ReadyChoice);
            AllChoicesReady?.Invoke(this, EventArgs.Empty);
        }

        private void ReadyChoice(StoryChoice storyChoice) {
            ChoiceHandler handler = GetChoiceHandler(storyChoice);

            if (handler == null) {
                // Debug.LogWarning($"No valid handler for story choice '{storyChoice.Key}");
                return;
            }

            m_CurrentChoiceHandlers[storyChoice] = handler;
            handler.Ready(storyChoice);
        }

        private ChoiceHandler GetChoiceHandler(StoryChoice storyChoice) {
            return Bindings.FirstOrDefault(b => storyChoice.IsValidChoice(b.ChoiceKey))?.ChoiceHandler;
        }

        private void OnChoicesWaiting() {
            // This assumes 'Ready' comes before 'ReadyAndWaiting' and has the same choices.
            
            // Create separate list in case current handlers gets cleared out.
            ChoiceHandler[] handlers = m_CurrentChoiceHandlers.Values.ToArray();
            
            handlers.ForEach(h => h.ReadyAndWaiting());
        }

        private void OnChoiceEvaluating(ChoiceHandler handler) {
            m_CurrentChoiceHandlers.Values.ForEach(h => h.PauseIf(h != handler));
            if (handler.InterruptsOnEvaluation) {
                m_StoryTeller.InterruptSequences();
            }
        }

        private void OnChoiceEvaluationFailed(ChoiceHandler handler) {
            // handler.Resume() can clear out the current choice handlers, so we are setting aside an array that won't change.
            ChoiceHandler[] handlers = m_CurrentChoiceHandlers.Values.ToArray();
            handlers.ForEach(h => h.Resume());
        }

        private void OnChoosing(StoryChoice storyChoice) {
            foreach (KeyValuePair<StoryChoice, ChoiceHandler> kvp in m_CurrentChoiceHandlers) {
                if (kvp.Key != storyChoice) {
                    kvp.Value.Cancel(storyChoice);
                } else if (kvp.Value.InterruptsOnChosen) {
                    // If the chosen ChoiceHandler interrupts running sequences when chosen, then interrupt sequences.
                    m_StoryTeller.InterruptSequences();
                }
            }
        }

        private void OnChosen() {
            m_CurrentChoiceHandlers.Values.ForEach(h => h.Cancel());
            m_CurrentChoiceHandlers.Clear();
        }

        public static bool IsValidChoice(ChoiceHandler handler) {
            return Instance.m_CurrentChoiceHandlers.Values.Contains(handler);
        }

        public static DelaySequence GetChoiceDelay(StoryChoice choice) {
            if (Instance.m_CurrentChoiceHandlers.TryGetValue(choice, out ChoiceHandler handler)) {
                return handler.ChoiceDelay;
            }

            // Debug.LogWarning($"Couldn't find current handler for {choice}", Instance);
            return DelaySequence.Empty;
        }
    }
}