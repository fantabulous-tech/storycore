using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameEvents {
    public class DestroyOnAwake : MonoBehaviour {
        private void Awake() {
            UnityUtils.DestroyObject(this);
        }
    }
}