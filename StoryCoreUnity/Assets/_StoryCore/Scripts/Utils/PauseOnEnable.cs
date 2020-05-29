using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.UI {
    public class PauseOnEnable : MonoBehaviour {
        [SerializeField] private float m_PauseSpeed = 0.0001f;

        private void OnEnable() {
            OnPauseChanged(true);
        }

        private void OnDisable() {
            OnPauseChanged(false);
        }

        private void OnPauseChanged(bool isPaused) {
            Time.timeScale = isPaused ? m_PauseSpeed : 1;
        }
    }
}