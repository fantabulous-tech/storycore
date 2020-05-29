using System;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command")]
    public class CommandHandler : BaseGameEvent<CommandHandler, string> {
        public string CommandDescription => m_EventDescription;

        public virtual bool OnQueue(ScriptCommandInfo commandInfo) {
            return true;
        }

        public bool TryRun(ScriptCommandInfo info, Action callback) {
            try {
                Run(info).Then(callback);
                return true;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return false;
            }
        }

        public virtual DelaySequence Run(ScriptCommandInfo info) {
            info.Params.ForEach(Raise);
            return DelaySequence.Empty;
        }

        protected override void RaiseDefault() {
            base.RaiseDefault();
            Run(new ScriptCommandInfo(Name));
        }
    }
}