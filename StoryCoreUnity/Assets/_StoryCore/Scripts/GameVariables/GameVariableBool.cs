using StoryCore.Utils;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Bool", order = (int) MenuOrder.VariableBool)]
    public class GameVariableBool : BaseGameVariable<GameVariableBool, bool> {
        protected override bool Parse(string stringValue) {
            return stringValue.IsNullOrEmpty() ? m_InitialValue : bool.Parse(stringValue);
        }
    }
}