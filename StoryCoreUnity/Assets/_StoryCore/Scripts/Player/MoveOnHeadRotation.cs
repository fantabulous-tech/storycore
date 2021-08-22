using System;
using CoreUtils;
using CoreUtils.GameVariables;
using UnityEngine;

namespace StoryCore {
    public class MoveOnHeadRotation : MonoBehaviour {
        [SerializeField] private Vector3 m_MinOffset = Vector3.zero;
        [SerializeField] private Vector3 m_MaxOffset = new Vector3(0, 1, 0);
        [SerializeField] private float m_Speed = 1;
        [SerializeField, AutoFillAsset] private GameVariableTransform m_HeadLocation;
        [SerializeField, AutoFillAsset] private GameVariableFloatRange m_ProgressTracker;

        private Transform m_Head;
        private Vector3 m_StartPos;
        private Vector3 m_MinPos;
        private Vector3 m_MaxPos;
        private Vector3 m_LastForward;
        private Vector3 m_LastMinOffset;
        private Vector3 m_LastMaxOffset;

        public float Progress {
            get => m_ProgressTracker.Progress;
            set => m_ProgressTracker.Progress = value;
        }

        private void Start() {
            m_StartPos = transform.localPosition;
            m_MinPos = m_StartPos + m_MinOffset;
            m_MaxPos = m_StartPos + m_MaxOffset;
            m_LastForward = m_Head ? m_Head.forward : Vector3.forward;
            OnHeadChanged(m_HeadLocation.Value);
            m_HeadLocation.Changed += OnHeadChanged;
        }

        private void OnHeadChanged(Transform head) {
            m_Head = head;
            m_LastForward = head ? head.forward : Vector3.forward;
        }

        private void FixedUpdate() {
            if (!m_Head) {
                return;
            }

#if UNITY_EDITOR
            if (!m_LastMaxOffset.Approximately(m_MaxOffset) || !m_LastMinOffset.Approximately(m_MinOffset)) {
                m_LastMinOffset = m_MinOffset;
                m_LastMaxOffset = m_MaxOffset;
                m_MinPos = m_StartPos + m_MinOffset;
                m_MaxPos = m_StartPos + m_MaxOffset;
            }
#endif

            Vector3 newForward = m_Head.forward;
            float delta = Vector3.SignedAngle(m_LastForward, newForward, m_Head.right);

            Progress = Mathf.Clamp01(Progress + delta*m_Speed);
            transform.localPosition = Vector3.Lerp(m_MinPos, m_MaxPos, Progress);

            m_LastForward = newForward;
        }
    }

}