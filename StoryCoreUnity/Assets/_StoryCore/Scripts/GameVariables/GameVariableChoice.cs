using System;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Choice", order = (int) MenuOrder.VariableString)]
    public class GameVariableChoice : BaseGameVariable<GameVariableChoice, StoryChoice> {
        protected override StoryChoice Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for choices.");
        }
    }
}