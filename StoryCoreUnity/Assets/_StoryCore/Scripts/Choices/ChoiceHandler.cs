using System;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.Choices {
    [CreateAssetMenu(menuName = "Choices/GenericChoice", fileName = "ChoiceGeneric", order = 0)]
    public class ChoiceHandler : ScriptableObject {
        [SerializeField] private bool m_InterruptsOnEvaluation;
        [SerializeField] private bool m_InterruptsOnChosen;
        [SerializeField] private float m_ChoiceDelay;

        [NonSerialized] protected StoryChoice m_Choice;
        [NonSerialized] private bool m_Init;
        [NonSerialized] private bool m_Evaluating;

        public event Action<ChoiceHandler> ChoiceEvaluating;
        public event Action<ChoiceHandler> ChoiceEvaluationFailed;
        public event Action<ChoiceHandler> ChoiceChosen;

        public UnityEvent OnChosen;

        [field: NonSerialized]
        protected bool IsPaused { get; private set; }
        
        [field: NonSerialized]
        protected bool IsWaiting { get; private set; }

        public bool InterruptsOnChosen => m_InterruptsOnChosen;
        public bool InterruptsOnEvaluation => m_InterruptsOnEvaluation;
        public DelaySequence ChoiceDelay => Delay.For(m_ChoiceDelay, this);

        public void TryInit() {
            if (!m_Init) {
                Init();
            }
        }

        protected virtual void Init() {
            m_Init = true;
        }

        public virtual void Ready(StoryChoice choice) {
            TryInit();
            m_Choice = choice;
            IsWaiting = false;
            CheckEvaluating();
        }

        public virtual void ReadyAndWaiting() {
            IsWaiting = true;
        }

        private void CheckEvaluating() {
            if (m_Evaluating) {
                Debug.LogWarning($"Evaluating didn't get reset on {name} somehow.", this);
            }
        }

        public void PauseIf(bool value) {
            if (value) {
                Pause();
            }
        }

        public void Pause() {
            if (IsPaused) {
                return;
            }

            IsPaused = true;
            PauseInternal();
            CheckEvaluating();
        }

        protected virtual void PauseInternal() { }

        public void Resume() {
            if (!IsPaused) {
                return;
            }
            IsPaused = false;
            ResumeInternal();
            CheckEvaluating();
        }

        protected virtual void ResumeInternal() { }

        public void Cancel(StoryChoice choice = null) {
            if (m_Choice == choice || choice == null) {
                m_Choice = null;
                IsWaiting = false;
                IsPaused = false;
            }

            CancelInternal(choice);
        }

        protected virtual void CancelInternal(StoryChoice choice) { }

        public void Choose() {
            if (m_Choice == null) {
                Debug.LogWarning($"Can't select choice {name} as no choice has been assigned. (No choice available?)");
                return;
            }

            m_Choice.Choose();
            RaiseChosen();
            ChooseInternal();
        }

        protected virtual void ChooseInternal() { }

        private void RaiseChosen() {
            m_Evaluating = false;
            OnChosen.Invoke();
            ChoiceChosen?.Invoke(this);
        }

        protected void RaiseEvaluating() {
            m_Evaluating = true;
            ChoiceEvaluating?.Invoke(this);
            // StoryDebug.Log($"{name}: Evaluating...", this);
        }

        protected void RaiseEvaluationFailed() {
            m_Evaluating = false;
            ChoiceEvaluationFailed?.Invoke(this);
            // StoryDebug.Log($"{name}: Evaluation FAILED.", this);
        }
    }
}