using UnityEngine;
using UnityEngine.Events;
using VRTK;

namespace StoryCore.Utils {
    public class TriggerOnPlayerEvents : MonoBehaviour {
        [SerializeField] private VRTK_PlayerObject.ObjectTypes m_ObjectType = VRTK_PlayerObject.ObjectTypes.Headset;

        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        private void OnTriggerEnter(Collider other) {
            VRTK_PlayerObject obj = other.GetComponent<VRTK_PlayerObject>();

            if (obj && obj.objectType == m_ObjectType) {
                OnEnter.Invoke();
            }
        }

        private void OnTriggerExit(Collider other) {
            VRTK_PlayerObject obj = other.GetComponent<VRTK_PlayerObject>();

            if (obj && obj.objectType == m_ObjectType) {
                OnExit.Invoke();
            }
        }
    }
}