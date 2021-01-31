using CoreUtils;
using CoreUtils.AssetBuckets;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command SetVariable")]
    public class CommandSetVariable : CommandHandler {

        [SerializeField, AutoFillAsset(DefaultName = "Game Variable Bucket")]
        private GameVariableBucket m_GameVariableBucket;

        public override DelaySequence Run(ScriptCommandInfo info) {
            if (info.Params.Length != 2) {
                Debug.LogWarning("/setvar: Wrong number of parameters specified.");
                return DelaySequence.Empty;
            }

            string variableName = info.Params[0];
            string value = info.Params[1];

            BaseGameVariable gameVariable = m_GameVariableBucket.Get(variableName);
            if (gameVariable == null) {
                Debug.LogWarning($"/setvar {variableName} variable not found.");
                return DelaySequence.Empty;
            }

            GameVariableBool gameVariableBool = gameVariable as GameVariableBool;

            if (gameVariableBool == null) {
                Debug.LogWarning($"/setvar {variableName} Only GameVariableBool currently supported.");
                return DelaySequence.Empty;
            }

            gameVariableBool.Value = value.ToLower() == "true";

            return DelaySequence.Empty;
        }
    }
}