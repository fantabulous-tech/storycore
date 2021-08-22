using System;
using CoreUtils;
using UnityEngine;
using UnityEngine.Events;

namespace StoryCore {
    public class MovePointEvents : MonoBehaviour {
        [SerializeField, AutoFill] private MovePoint m_MovePoint;

        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        
        [Header("States")]

        public UnityEvent OnInactive;
        public UnityEvent OnNormal;
        public UnityEvent OnHover;

        private void Start() {
            m_MovePoint.Activate += OnActivate.Invoke;
            m_MovePoint.Deactivate += OnDeactivate.Invoke;
            m_MovePoint.StateChange += OnStateChange;
        }

        private void OnDestroy() {
            if (m_MovePoint != null) {
                m_MovePoint.Activate -= OnActivate.Invoke;
                m_MovePoint.Deactivate -= OnDeactivate.Invoke;
                m_MovePoint.StateChange -= OnStateChange;
            }
        }

        private void OnStateChange(MovePoint.MoveStates state) {
            switch (state) {
                case MovePoint.MoveStates.Inactive:
                    OnInactive.Invoke();
                    break;
                case MovePoint.MoveStates.Normal:
                    OnNormal.Invoke();
                    break;
                case MovePoint.MoveStates.Hover:
                    OnHover.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}