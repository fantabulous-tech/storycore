using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoryCore.Utils;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Script")]
    public class CommandScriptGameEvent : BaseGameEvent<CommandScriptGameEvent, ScriptCommandInfo> { }

    [Serializable]
    public class ScriptCommandInfo {
        private static readonly Regex s_ParamRegex = new Regex(@"(?:(?<key>[\w-]+)\s*=\s*)?(?:""(?<value>(?:[^""]|(?:\\""))+(?<!\\))""|(?<value>[^\s]+))");

        public string Text { get; private set; }
        public string Command { get; private set; }
        public string[] Params { get; private set; }
        public Dictionary<string, string> NamedParams { get; private set; }

        public ScriptCommandInfo(string commandText) {
            Text = commandText;
            Match[] matches = s_ParamRegex.Matches(commandText).Cast<Match>().ToArray();
            Command = matches.First().Value.TrimStart('/');
            Params = matches.Skip(1).Select(m => m.Value).ToArray();

            NamedParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in matches) {
                if (m.Groups["key"].Value.IsNullOrEmpty()) {
                    continue;
                }
                AssignKeyValuePair(m.Groups["key"].Value, m.Groups["value"].Value);
            }

            NamedParams = matches.Where(m => !m.Groups["key"].Value.IsNullOrEmpty()).ToDictionary(m => m.Groups["key"].Value, m => m.Groups["value"].Value, StringComparer.OrdinalIgnoreCase);
        }

        public ScriptCommandInfo(string command, string[] args) {
            Command = command;
            Params = args;
            NamedParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string arg in args) {
                if (arg.Contains('=')) {
                    string[] pieces = arg.SplitRegex(@"\s*=\s*");
                    if (pieces.Length != 2) {
                        continue;
                    }
                    AssignKeyValuePair(pieces[0], pieces[1]);
                }
            }
        }

        private void AssignKeyValuePair(string key, string value) {
            if (NamedParams.ContainsKey(key)) {
                Debug.LogWarning($"Duplicate parameter found: '{key}'. Value was not used: '{value}'");
            } else {
                NamedParams[key] = value;
            }
        }

        public override string ToString() {
            return $"Ink Command: {Command} {Params.AggregateToString(" ")}";
        }
    }
}