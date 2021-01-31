using System;
using CoreUtils.GameEvents;
using StoryCore;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Spawn Point", order = (int) MenuOrder.VariableString)]
    public class GameVariableSpawnPoint : BaseGameVariable<GameVariableSpawnPoint, SpawnPoint> {
        protected override SpawnPoint Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for spawn points.");
        }
    }
}