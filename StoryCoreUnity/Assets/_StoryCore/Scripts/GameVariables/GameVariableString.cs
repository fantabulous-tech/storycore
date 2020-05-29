using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/String", order = (int) MenuOrder.VariableString)]
    public class GameVariableString : BaseGameVariable<GameVariableString, string> {
        protected override string Parse(string stringValue) {
            return stringValue;
        }
    }
}