using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Bool", order = (int) MenuOrder.EventBool)]
    public class GameEventBool : BaseGameEvent<GameEventBool, bool> { }
}