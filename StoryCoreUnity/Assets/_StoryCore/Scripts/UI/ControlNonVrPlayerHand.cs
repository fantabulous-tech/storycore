using CoreUtils;
using StoryCore.UI;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class ControlNonVrPlayerHand : MonoBehaviour {
        [SerializeField] private bool m_EnablePointing = true;
        [SerializeField] private SDK_InputSimulator m_InputSimulator;
        [SerializeField] private Vector3 m_CloseOffset = Vector3.down*0.1f;
        [SerializeField] private float m_ShiftSpeed = 0.3f;
        [SerializeField] private Vector3 m_RotationOffset;

        private VRTK_Pointer m_Pointer;
        private Vector3 m_StartPos;
        private Quaternion m_StartRot;
        private Vector3 m_Velocity;

        private Vector3 ClosePose => transform.parent.InverseTransformPoint(UnityUtils.CameraTransform.position) + m_CloseOffset;

        private Vector3 TargetPose => CrosshairTarget.Distance > 0 ? Vector3.Lerp(ClosePose, m_StartPos, Mathf.Clamp01(CrosshairTarget.Distance)) : m_StartPos;
        public bool DisableHandUpdate { get; set; }

        private void Start() {
            m_Pointer = GetComponentInChildren<VRTK_Pointer>();
            Transform t = transform;
            m_StartPos = t.localPosition;
            m_StartRot = t.localRotation;
        }

        private void LateUpdate() {
            if (DisableHandUpdate || m_InputSimulator && m_InputSimulator.IsHand || Input.anyKey) {
                return;
            }

            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, TargetPose, ref m_Velocity, m_ShiftSpeed, 10, Time.unscaledTime);

            if (m_EnablePointing && CrosshairTarget.Target) {
                transform.LookAt(CrosshairTarget.Point);
                if (m_Pointer) {
                    transform.localRotation = transform.localRotation*Quaternion.Inverse(m_Pointer.transform.localRotation)*Quaternion.Euler(m_RotationOffset);
                }
            } else {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_StartRot, m_ShiftSpeed);
            }
        }
    }
}