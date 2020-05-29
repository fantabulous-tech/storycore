using System;
using System.Linq;
using StoryCore.GameEvents;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Action")]
    public class CommandActionHandler : CommandHandler {
        [SerializeField] private ActionBindingBucket m_ActionBucket;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string actionName = info.Params.GetFirst();
            string actionParams = info.Params.Length > 1 ? info.Params.Skip(1).AggregateToString() : null;

            ActionBindingBucket.ActionBinding binding = m_ActionBucket.ActionBindings.FirstOrDefault(ab => ab.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase));

            if (binding == null) {
                Debug.LogWarningFormat(this, $"Couldn't find event binding for {actionName}.");
                return DelaySequence.Empty;
            }

            if (binding.GameEvent == null) {
                Debug.LogWarningFormat(this, $"Event on binding for {actionName} is missing.");
                return DelaySequence.Empty;
            }

            Debug.LogFormat(this, "Generic Command: Raising '" + binding.GameEvent.name + "'");

            GameEventString gameEventString = binding.GameEvent as GameEventString;

            if (gameEventString != null && !actionParams.IsNullOrEmpty()) {
                gameEventString.Raise(actionParams);
                return DelaySequence.Empty;
            }

            GameVariableString gameVariableString = binding.GameEvent as GameVariableString;

            if (gameVariableString != null && !actionParams.IsNullOrEmpty()) {
                gameVariableString.Value = actionParams;
                return DelaySequence.Empty;
            }

            binding.GameEvent.Raise();
            return DelaySequence.Empty;
        }
    }
}