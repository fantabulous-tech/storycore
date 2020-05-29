using StoryCore.Utils;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Int", order = (int) MenuOrder.VariableInt)]
    public class GameVariableInt : BaseGameVariable<GameVariableInt, int> {
        protected override int Parse(string stringValue) {
            return stringValue.IsNullOrEmpty() ? 0 : int.Parse(stringValue);
        }
    }
}