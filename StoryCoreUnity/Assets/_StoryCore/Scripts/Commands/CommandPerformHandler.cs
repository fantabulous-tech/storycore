using System.Linq;
using StoryCore.Characters;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Perform")]
    public class CommandPerformHandler : CommandHandler {
        [SerializeField] private GameVariableString m_FocusedCharacter;

        public BaseCharacter FocusedCharacter => m_FocusedCharacter ? Buckets.Characters.Get(m_FocusedCharacter.Value)?.GetComponent<BaseCharacter>() : null;

        public override DelaySequence Run(ScriptCommandInfo info) {
            // Find the performer.
            BaseCharacter performer = FocusedCharacter;

            // Pass on parameters to '/perform' command.
            if (info.Params.Length > 1) {
                CommandManager.RunCommand("/emotion " + info.Params.Skip(1).AggregateToString(" "));
            }

            if (!performer) {
                Debug.LogWarningFormat(this, "No focused character, so can't perform '{0}'", info.Params.AggregateToString(" "));
                return DelaySequence.Empty;
            }

            return performer.Perform(info);
        }
    }
}