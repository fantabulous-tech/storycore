using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/String", order = (int) MenuOrder.EventString)]
    public class GameEventString : BaseGameEvent<GameEventString, string> { }
}