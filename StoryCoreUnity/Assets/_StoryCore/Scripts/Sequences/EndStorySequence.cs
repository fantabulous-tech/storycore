namespace StoryCore {
    public class EndStorySequence : ISequence {
        private readonly StoryTeller m_StoryTeller;

        public bool IsComplete { get; private set; }

        public EndStorySequence(StoryTeller storyTeller) {
            m_StoryTeller = storyTeller;
        }

        public void OnQueue() { }

        public void Start() {
            m_StoryTeller.EndStory();
            IsComplete = true;
        }

        public void Cancel() { }
    }
}