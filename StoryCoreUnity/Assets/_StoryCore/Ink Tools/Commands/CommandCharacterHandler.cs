using System.Linq;
using CoreUtils;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Character")]
    public class CommandCharacterHandler : CommandHandler {
        [SerializeField] private GameVariableString m_FocusedCharacter;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string characterName = info.Params[0];
            StoryDebug.LogFormat(this, "Character Command: Focusing on '" + characterName + "'");
            m_FocusedCharacter.Value = characterName;

            // Pass on parameters to '/perform' command.
            if (info.Params.Length > 1) {
                CommandManager.RunCommand("/perform " + info.Params.Skip(1).AggregateToString(" "));
            }

            return DelaySequence.Empty;
        }
    }
}