using System;
using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Spawn Point", order = (int) MenuOrder.VariableString)]
    public class GameVariableSpawnPoint : BaseGameVariable<GameVariableSpawnPoint, SpawnPoint> {
        protected override SpawnPoint Parse(string stringValue) {
            throw new NotImplementedException("No serialization system for spawn points.");
        }
    }
}