using System.Linq;
using StoryCore.Characters;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command LookAt")]
    public class CommandLookAtHandler : CommandHandler {
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;

        private BaseCharacter FocusedCharacter => m_StoryTellerLocator && m_StoryTellerLocator.Value ? m_StoryTellerLocator.Value.FocusedCharacter : null;

        public override DelaySequence Run(ScriptCommandInfo info) {
            string targetName = info.Params.FirstOrDefault() ?? "";
            Transform target;

            switch (targetName.ToLower()) {
                case "none":
                    target = null;
                    break;
                case "player":
                    target = UnityUtils.CameraTransform;
                    break;
                default:
                    BaseCharacter targetCharacter = Buckets.Characters.Get(targetName);
                    target = targetCharacter ? targetCharacter.AttentionPoint : null;
                    if (!targetCharacter) {
                        Debug.LogWarning($"/lookat: '{targetName}' not found. Looking at nothing instead.");
                    }
                    break;
            }

            // TODO: Support non-character and 'idle' points of interest.

            if (FocusedCharacter) {
                Debug.Log($"/lookat: {FocusedCharacter.Name} --> {targetName} ({target})");
                FocusedCharacter.LookAt(target);
            } else {
                Debug.LogWarning($"/lookat: No focused character found. Cannot look at '{targetName}'");
            }

            return DelaySequence.Empty;
        }
    }
}