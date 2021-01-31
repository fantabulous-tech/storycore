using CoreUtils;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class Player : MonoBehaviour {
        public static Player s_Instance;

        private void Awake() {
            if (s_Instance && s_Instance != this) {
                Debug.LogError("More than one player found in scene!", this);
                return;
            }

            s_Instance = this;
        }
    }
}