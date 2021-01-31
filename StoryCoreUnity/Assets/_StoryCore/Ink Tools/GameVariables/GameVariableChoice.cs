using System;
using CoreUtils.GameEvents;
using StoryCore;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Choice", order = (int) MenuOrder.VariableString)]
    public class GameVariableChoice : BaseGameVariable<GameVariableChoice, StoryChoice> {
        protected override StoryChoice Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for choices.");
        }
    }
}