using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.Utils {
    public class TriggerEvents : MonoBehaviour {
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        private void OnTriggerEnter(Collider other) {
            OnEnter.Invoke();
        }

        private void OnTriggerExit(Collider other) {
            OnExit.Invoke();
        }
    }
}