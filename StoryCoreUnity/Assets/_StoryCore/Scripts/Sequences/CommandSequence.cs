namespace StoryCore {
    internal class CommandSequence : ISequence {
        private readonly StoryTeller m_StoryTeller;
        private readonly string m_Text;

        public bool IsComplete { get; private set; }

        public CommandSequence(StoryTeller storyTeller, string text) {
            m_Text = text;
            m_StoryTeller = storyTeller;
        }

        public void OnQueue() {
            CommandManager.QueueCommand(m_Text);
        }

        public void Start() {
            Log($"RUN COMMAND: {m_Text}");
            CommandManager.RunCommand(m_Text, () => IsComplete = true, () => IsComplete = true);
        }

        public void Cancel() {
            // Required for interface.
        }

        private void Log(string log) {
            m_StoryTeller.Log(log);
        }
    }
}