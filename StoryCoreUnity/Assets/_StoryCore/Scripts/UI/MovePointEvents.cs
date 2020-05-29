using UnityEngine;
using UnityEngine.Events;

namespace StoryCore {
    public class MovePointEvents : MonoBehaviour {
        [SerializeField] private MovePoint m_MovePoint;

        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;

        private void Start() {
            m_MovePoint.Activate += OnActivate.Invoke;
            m_MovePoint.Deactivate += OnDeactivate.Invoke;
        }

        private void OnDestroy() {
            if (m_MovePoint != null) {
                m_MovePoint.Activate -= OnActivate.Invoke;
                m_MovePoint.Deactivate -= OnDeactivate.Invoke;
            }
        }
    }
}