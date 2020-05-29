using UnityEngine;

namespace StoryCore.Utils {
    public class DisableInRelease : MonoBehaviour {
        private void Awake() {
            if (!Debug.isDebugBuild) {
                gameObject.SetActive(false);
            }
        }
    }
}