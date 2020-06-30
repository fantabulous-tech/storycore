using System;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Vector3", order = (int) MenuOrder.VariableVector3)]
    public class GameVariableVector3 : BaseGameVariable<GameVariableVector3, Vector3> {
        protected override Vector3 Parse(string stringValue) {
            char[] seperators = {',', ' '};
            string[] parts = stringValue.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            // will throw exception on bad formatting
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        protected override bool Equals(Vector3 a, Vector3 b) {
            return a.Approximately(b);
        }
    }
}