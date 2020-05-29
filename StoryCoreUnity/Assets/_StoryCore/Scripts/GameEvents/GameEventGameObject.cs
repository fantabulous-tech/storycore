using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/GameObject", order = (int) MenuOrder.EventGameObject)]
    public class GameEventGameObject : BaseGameEvent<GameEventGameObject, GameObject> { }
}