using CoreUtils;
using CoreUtils.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Send GameEvent")]
    public class CommandSendGameEventHandler : CommandHandler {
        [SerializeField] private BaseGameEvent m_Event;

        public override DelaySequence Run(ScriptCommandInfo info) {
            Debug.LogFormat(this, "Generic Command: Raising '" + m_Event.name + "'");
            m_Event.Raise();
            return DelaySequence.Empty;
        }
    }
}