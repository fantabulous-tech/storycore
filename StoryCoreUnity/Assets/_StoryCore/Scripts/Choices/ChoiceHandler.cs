using System;
using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.Choices {
    [CreateAssetMenu(menuName = "Choices/GenericChoice", fileName = "Choice", order = 0)]
    public class ChoiceHandler : ScriptableObject {
        [NonSerialized] private StoryChoice m_Choice;
        [NonSerialized] private bool m_Paused;
        [NonSerialized] private bool m_Init;

        private bool m_Evaluating;

        public event Action<ChoiceHandler> ChoiceEvaluating;
        public event Action<ChoiceHandler> ChoiceEvaluationFailed;
        public event Action<ChoiceHandler> ChoiceChosen;

        public UnityEvent OnChosen;

        public virtual void Ready(StoryChoice choice) {
            TryInit();
            m_Choice = choice;
            CheckEvaluating();
        }

        private void CheckEvaluating() {
            if (m_Evaluating) {
                Debug.LogWarning("Evaluating didn't get reset somehow.");
            }
        }

        public virtual void ReadyAndWaiting(StoryChoice choice) {
            TryInit();
            m_Choice = choice;
            CheckEvaluating();
        }

        public void TryInit() {
            if (!m_Init) {
                Init();
            }
        }

        protected virtual void Init() {
            m_Init = true;
        }

       public void PauseIf(bool value) {
            if (value) {
                Pause();
            }
        }

        public void Pause() {
            if (m_Paused) {
                return;
            }

            m_Paused = true;
            PauseInternal();
            CheckEvaluating();
        }

        protected virtual void PauseInternal() { }

        public void Resume() {
            if (!m_Paused) {
                return;
            }
            m_Paused = false;
            ResumeInternal();
            CheckEvaluating();
        }

        protected virtual void ResumeInternal() { }

         public virtual void Cancel() {
            m_Choice = null;
            CheckEvaluating();
        }

        public void Choose() {
            if (m_Choice == null) {
                Debug.LogWarning($"Can't select choice {name} as no choice has been assigned. (No choice available?)");
                return;
            }
            
            m_Choice.Choose();
            RaiseChosen();
        }

        private void RaiseChosen() {
            m_Evaluating = false;
            OnChosen.Invoke();
            ChoiceChosen?.Invoke(this);
        }

        protected void RaiseEvaluating() {
            m_Evaluating = true;
            ChoiceEvaluating?.Invoke(this);
        }

        protected void RaiseEvaluationFailed() {
            m_Evaluating = false;
            ChoiceEvaluationFailed?.Invoke(this);
        }
    }
}