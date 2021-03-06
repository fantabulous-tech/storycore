using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Quit", fileName = "CommandQuit")]
    public class CommandQuitHandler : CommandHandler {
        public override DelaySequence Run(ScriptCommandInfo info) {
            Delay.OneFrame(this).Then(UnityUtils.Quit);
            return DelaySequence.Empty;
        }
    }
}