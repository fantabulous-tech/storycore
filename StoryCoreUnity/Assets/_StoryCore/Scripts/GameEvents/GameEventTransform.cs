using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Transform", order = (int) MenuOrder.EventTransform)]
    public class GameEventTransform : BaseGameEvent<GameEventTransform, Transform> { }
}