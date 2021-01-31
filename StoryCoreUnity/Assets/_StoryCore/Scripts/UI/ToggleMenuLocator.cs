using System;
using CoreUtils.GameEvents;
using StoryCore.UI;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/ToggleMenu", order = (int) MenuOrder.VariableString)]
    public class ToggleMenuLocator : BaseGameVariable<ToggleMenuLocator, ToggleMenu> {
        protected override ToggleMenu Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for StoryTeller.");
        }
    }
}