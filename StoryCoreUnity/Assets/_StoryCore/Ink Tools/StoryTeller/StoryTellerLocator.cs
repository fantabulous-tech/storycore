using System;
using CoreUtils.GameEvents;
using StoryCore;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/StoryTeller", order = (int) MenuOrder.VariableString)]
    public class StoryTellerLocator : BaseGameVariable<StoryTellerLocator, StoryTeller> {
        protected override StoryTeller Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for StoryTeller.");
        }
    }
}