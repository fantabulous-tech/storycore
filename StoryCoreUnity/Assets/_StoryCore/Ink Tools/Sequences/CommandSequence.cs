using System.Collections.Generic;
using StoryCore.Commands;

namespace StoryCore {
    internal class CommandSequence : ISequence {
        private readonly string m_Text;
        private readonly List<string> m_StoryTags;
        private readonly ScriptCommandInfo m_CommandInfo;

        public bool IsComplete { get; private set; }
        public bool AllowsChoices => CommandManager.AllowsChoices(m_CommandInfo.Command);

        public CommandSequence(string text, List<string> storyTags) {
            m_Text = text;
            m_StoryTags = storyTags;
            m_CommandInfo = CommandManager.QueueCommand(m_Text);
        }

        public void Start() {
            StoryDebug.Log($"RUN COMMAND: {m_Text}");
            CommandManager.RunCommand(m_Text, m_StoryTags, () => IsComplete = true, () => IsComplete = true);
        }

        public void Interrupt() {
            // Required for interface.
        }

        public void Cancel() {
            // Required for interface.
        }

        public override string ToString() {
            return $"{base.ToString()}: {m_Text}";
        }
    }
}