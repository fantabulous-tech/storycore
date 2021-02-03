using System;

namespace StoryCore {
    public class WaitForChoiceSequence : ISequence {
        private readonly StoryTeller m_StoryTeller;
        private readonly Action m_ChoicesWaitingAction;

        public bool IsComplete { get; private set; }
        public bool AllowsChoices => true;

        public WaitForChoiceSequence(StoryTeller storyTeller, Action choicesWaitingAction) {
            m_StoryTeller = storyTeller;
            m_StoryTeller.OnChosen += OnChosen;
            m_ChoicesWaitingAction = choicesWaitingAction;
        }

        ~WaitForChoiceSequence() {
            if (m_StoryTeller) {
                m_StoryTeller.OnChosen -= OnChosen;
            }
        }

        public void Start() {
            m_ChoicesWaitingAction?.Invoke();
        }

        public void Interrupt() { 
            // Required for interface.
        }

        public void Cancel() {
            m_StoryTeller.OnChosen -= OnChosen;
        }

        private void OnChosen() {
            m_StoryTeller.OnChosen -= OnChosen;
            IsComplete = true;
        }
    }
}