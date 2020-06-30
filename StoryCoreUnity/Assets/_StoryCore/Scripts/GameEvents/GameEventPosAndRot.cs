// ---------------------------------------------------------------------
// ---------------------------------------------------------------------

using UnityEngine;

namespace StoryCore.GameEvents {

    public struct PosAndRot {
        public Vector3 position;
        public Quaternion rotation;
    }

    [CreateAssetMenu(menuName = "GameEvent/PosAndRot", order = (int) MenuOrder.EventPosAndRot)]
    public class GameEventPosAndRot : BaseGameEvent<GameEventPosAndRot, PosAndRot> { }
}