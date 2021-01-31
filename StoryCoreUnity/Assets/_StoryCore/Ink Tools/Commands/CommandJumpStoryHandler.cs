using System.Linq;
using CoreUtils;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Jump Story")]
    public class CommandJumpStoryHandler : CommandHandler {
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string path = info.Params.ElementAtOrDefault(0);
            m_StoryTellerLocator.Value.JumpStory(path);
            return DelaySequence.Empty;
        }
    }
}