using CoreUtils;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Story Function")]
    public class CommandStoryFunctionHandler : CommandHandler {
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField] private string m_Function;

        private StoryTeller StoryTeller => m_StoryTellerLocator.Value;

        public override DelaySequence Run(ScriptCommandInfo info) {
            if (!StoryTeller) {
                Debug.LogWarning($"Couldn't call story function {m_Function}. No StoryTeller found.");
            } else {
                StoryTeller.Story.EvaluateFunction(m_Function);
            }

            return DelaySequence.Empty;
        }
    }
}