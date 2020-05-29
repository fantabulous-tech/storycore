using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameVariable/Story Int", order = (int) MenuOrder.VariableString)]
    public class GameVariableStoryInt : BaseGameVariableStory<GameVariableStoryInt, int> {
        protected override int Parse(string stringValue) {
            return int.Parse(stringValue);
        }

        protected override int GetValue() {
            if (m_Story == null) {
                GetStory();
            }

            return m_Story != null ? (int) m_Story.variablesState[name] : -1;
        }
    }
}