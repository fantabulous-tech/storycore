using UnityEngine;

namespace StoryCore.GameEvents {
    public class HideOnAwake : MonoBehaviour {
        private void Awake() {
            if (!gameObject.activeInHierarchy) {
                gameObject.SetActive(false);
            }
        }
    }
}