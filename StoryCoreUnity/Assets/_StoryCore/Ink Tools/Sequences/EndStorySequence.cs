namespace StoryCore {
    public class EndStorySequence : ISequence {
        private readonly StoryTeller m_StoryTeller;

        public bool IsComplete { get; private set; }
        public bool AllowsChoices => false;

        public EndStorySequence(StoryTeller storyTeller) {
            m_StoryTeller = storyTeller;
        }

        public void Start() {
            m_StoryTeller.EndStory();
            IsComplete = true;
        }

        public void Interrupt() {
            // Required for interface.
        }

        public void Cancel() {
            // Required for interface.
        }
    }
}