using UnityEngine;

namespace StoryCore.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Generic", order = (int) MenuOrder.EventGeneric)]
    public class GameEvent : BaseGameEvent {
        protected override void RaiseDefault() {
            RaiseGeneric();
        }
    }
}