namespace StoryCore {
    public class ReadyForChoiceSequence : ISequence {
        private readonly StoryTeller m_StoryTeller;

        public bool IsComplete { get; set; }

        public ReadyForChoiceSequence(StoryTeller storyTeller) {
            m_StoryTeller = storyTeller;
        }

        public void OnQueue() { }

        public void Start() {
            m_StoryTeller.RaiseOnChoicesReadyAndWaiting();
        }

        public void Cancel() { }
    }
}