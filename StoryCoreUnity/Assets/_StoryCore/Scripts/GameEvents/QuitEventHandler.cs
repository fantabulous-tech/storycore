using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.UI {
    public class QuitEventHandler : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_QuitEvent;

        private void Start() {
            m_QuitEvent.GenericEvent += UnityUtils.Quit;
        }
    }
}