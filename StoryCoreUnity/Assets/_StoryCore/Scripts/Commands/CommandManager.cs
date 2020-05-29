using System;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class CommandManager : Singleton<CommandManager> {
        private const string kLock = "lock";
        private const string kUnlock = "unlock";
        private const string kCharacter = "character";

        #region Convenience Properties

        public static CommandHandler Lock {
            get => Instance.Commands[kLock];
            set => Instance.Commands[kLock] = value;
        }
        public static CommandHandler Unlock {
            get => Instance.Commands[kUnlock];
            set => Instance.Commands[kUnlock] = value;
        }
        public static CommandHandler Character {
            get => Instance.Commands[kCharacter];
            set => Instance.Commands[kCharacter] = value;
        }

        #endregion

        [SerializeField] private CommandKeyBindings m_Commands;

        private CommandKeyBindings Commands => UnityUtils.GetOrInstantiate(ref m_Commands);

        private void OnEnable() {
            Commands.Init();
        }

        public static void QueueCommand(string text) {
            Instance.m_Commands.QueueCommand(text);
        }

        public static void RunCommand(string text, Action callback = null, Action failCallback = null) {
            Instance.m_Commands.RunCommand(text, callback, failCallback);
        }
    }
}