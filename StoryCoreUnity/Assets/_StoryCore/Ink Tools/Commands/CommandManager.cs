using System;
using System.Collections.Generic;
using CoreUtils;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class CommandManager : Singleton<CommandManager> {
        [SerializeField] private CommandBucket m_Commands;

        public override void OnEnable() {
            base.OnEnable();
            m_Commands.Init();
        }

        public static ScriptCommandInfo QueueCommand(string text) {
            return Instance.m_Commands.QueueCommand(text);
        }

        public static ScriptCommandInfo RunCommand(string text, List<string> storyTags = null, Action callback = null, Action failCallback = null) {
            return Instance.m_Commands.RunCommand(text, storyTags, callback, failCallback);
        }

        public static bool AllowsChoices(string command) {
            CommandHandler commandHandler = Instance.m_Commands[command];

            if (commandHandler == null) {
                Debug.LogError($"Couldn't find command handler for command '{command}'.");
                return false;
            }

            return commandHandler.AllowsChoices;
        }
    }
}