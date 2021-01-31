using System;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Log")]
    public class CommandLogHandler : CommandHandler {
        private enum LogType {
            Log,
            Warning,
            Error
        }

        [SerializeField] private LogType m_Type;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string message = "INK LOG: " + info.Text.Substring(info.Text.IndexOf(' '));

            switch (m_Type) {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return DelaySequence.Empty;
        }
    }
}