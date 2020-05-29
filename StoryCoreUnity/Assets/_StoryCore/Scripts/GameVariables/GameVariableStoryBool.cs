using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameVariable/Story Bool", order = (int) MenuOrder.VariableString)]
    public class GameVariableStoryBool : BaseGameVariableStory<GameVariableStoryBool, bool> {
        protected override bool Parse(string stringValue) {
            if (int.TryParse(stringValue, out int result)) {
                return result > 0;
            }

            if (bool.TryParse(stringValue, out bool boolResult)) {
                return boolResult;
            }

            Debug.LogError($"Could not parse the string for GameVariableStoryBool {name}: {stringValue}", this);
            return false;
        }

        protected override bool GetValue() {
            if (m_Story == null) {
                GetStory();
            }

            return m_Story == null ? m_InitialValue : Parse(m_Story.variablesState[name].ToString());
        }
    }
}