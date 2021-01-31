using System;
using System.Collections.Generic;
using System.Linq;
using CoreUtils;
using IngameDebugConsole;
using JetBrains.Annotations;
using CoreUtils.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Buckets/Command Key Bindings")]
    public class CommandKeyBindings : BaseBucket {
        [SerializeField] private CommandScriptGameEvent m_CommandEvent;
        [SerializeField, HideInInspector] private CommandBinding[] m_CommandBindings;

        private Dictionary<string, CommandBinding> m_CommandLookup;

        private Dictionary<string, CommandBinding> CommandLookup => UnityUtils.GetOrSet(ref m_CommandLookup, () => m_CommandBindings.ToDictionary(b => b.CommandName, StringComparer.OrdinalIgnoreCase));

        private CommandBinding[] CommandBindings => UnityUtils.GetOrSet(ref m_CommandBindings, () => new CommandBinding[0]);

        public override string[] ItemNames => m_CommandBindings.Select(i => i.CommandName).ToArray();

        public CommandBinding[] GetCommandBindings() {
            return CommandBindings;
        }

        public void Init() {
            if (!AppTracker.IsPlaying) {
                return;
            }
            if (m_CommandEvent != null) {
                m_CommandEvent.Event += OnCommand;
            }
            CommandBindings.ForEach(c => c.RegisterWithDebugConsole());
        }

        private void OnCommand(ScriptCommandInfo commandInfo) {
            RunCommand(commandInfo);
        }

        public ScriptCommandInfo RunCommand(string commandText, List<string> tags, Action callback = null, Action failCallback = null) {
            ScriptCommandInfo commandInfo = new ScriptCommandInfo(commandText, tags);
            RunCommand(commandInfo, callback, failCallback);
            return commandInfo;
        }

        public CommandHandler this[string command] {
            get => CommandLookup.ContainsKey(command) ? CommandLookup[command].CommandHandler : null;
            set => CommandLookup[command].CommandHandler = value;
        }

        public ScriptCommandInfo QueueCommand(string commandText) {
            ScriptCommandInfo commandInfo = new ScriptCommandInfo(commandText);

            try {
                CommandHandler lastCommand = this[commandInfo.Command];

                if (lastCommand == null || !lastCommand.OnQueue(commandInfo)) {
                    Debug.LogWarning("Command not found: " + commandInfo);
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                Debug.LogErrorFormat(this, "Command {0} failed to queue because {1}", commandInfo, e.Message);
            }

            return commandInfo;
        }

        private void RunCommand(ScriptCommandInfo commandInfo, Action callback = null, Action failCallback = null) {
            try {
                CommandHandler lastCommand = this[commandInfo.Command];

                if (lastCommand == null || !lastCommand.TryRun(commandInfo, callback)) {
                    Debug.LogWarning("Command not found: " + commandInfo);
                    failCallback?.Invoke();
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                Debug.LogErrorFormat(this, "Command {0} failed because {1}", commandInfo, e.Message);
                failCallback?.Invoke();
            }
        }

        [Serializable]
        public class CommandBinding {
            [UsedImplicitly] public string CommandName;
            [UsedImplicitly] public CommandHandler CommandHandler;

            public void RegisterWithDebugConsole() {
                if (!CommandName.IsNullOrEmpty() && CommandHandler) {
                    DebugLogConsole.AddCommandInstance(CommandName, CommandHandler.CommandDescription, "Execute", this);
                }
            }

            [UsedImplicitly]
            public void Execute(params string[] args) {
                CommandHandler.Run(new ScriptCommandInfo(CommandName, args));
            }
        }

        public override bool Has(string itemName) {
            return CommandBindings.Any(b => b != null && b.CommandName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }
    }
}