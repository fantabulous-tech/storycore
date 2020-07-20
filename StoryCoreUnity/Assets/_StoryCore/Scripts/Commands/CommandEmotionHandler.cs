using StoryCore.Characters;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Emotion")]
    public class CommandEmotionHandler : CommandHandler {
        [SerializeField, AutoFillAsset] private GameVariableString m_FocusedCharacter;

        private BaseCharacter FocusedCharacter => m_FocusedCharacter ? Buckets.Characters.Get(m_FocusedCharacter.Value)?.GetComponent<BaseCharacter>() : null;

        public override DelaySequence Run(ScriptCommandInfo info) {
            // Find the performer.
            BaseCharacter performer = FocusedCharacter;

            if (!performer) {
                Debug.LogWarningFormat(this, "No focused character, so can't perform '{0}'", info.Params.AggregateToString(" "));
                return DelaySequence.Empty;
            }

            return performer.SetEmotion(info);
        }
    }
}