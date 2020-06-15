using System.Collections.Generic;

namespace StoryCore {
    internal class CommandSequence : ISequence {
        private readonly StoryTeller m_StoryTeller;
        private readonly string m_Text;
        private readonly List<string> m_StoryTags;

        public bool IsComplete { get; private set; }

        public CommandSequence(StoryTeller storyTeller, string text, List<string> storyTags) {
            m_Text = text;
            m_StoryTeller = storyTeller;
            m_StoryTags = storyTags;
        }

        public void OnQueue() {
            CommandManager.QueueCommand(m_Text);
        }

        public void Start() {
            Log($"RUN COMMAND: {m_Text}");
            CommandManager.RunCommand(m_Text, m_StoryTags, () => IsComplete = true, () => IsComplete = true);
        }

        public void Cancel() {
            // Required for interface.
        }

        private void Log(string log) {
            StoryDebug.Log(log);
        }
    }
}