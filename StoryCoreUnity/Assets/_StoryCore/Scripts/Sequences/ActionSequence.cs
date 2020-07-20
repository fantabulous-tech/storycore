using System;

namespace StoryCore {
    internal class ActionSequence : ISequence {
        private readonly Action m_Action;

        public bool IsComplete { get; private set; }
        public bool AllowsChoices { get; }

        public ActionSequence(Action action, bool allowsChoices = false) {
            m_Action = action;
            AllowsChoices = allowsChoices;
        }

        public void Start() {
            m_Action?.Invoke();
            IsComplete = true;
        }

        public void Interrupt() {
            // Required for interface.
        }

        public void Cancel() {
            // Required for interface.
        }

        public override string ToString() {
            return $"{base.ToString()}: Action = {m_Action.Method.Name}";
        }
    }
}