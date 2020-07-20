using StoryCore.UI;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class ControlNonVrPlayerHand : MonoBehaviour {
        [SerializeField] private bool m_EnablePointing = true;
        [SerializeField] private SDK_InputSimulator m_InputSimulator;
        [SerializeField] private Vector3 m_CloseOffset = Vector3.down*0.1f;
        [SerializeField] private float m_ShiftSpeed = 0.3f;
        [SerializeField] private Vector3 m_RotationOffset;
        [SerializeField] private CrosshairTarget m_CrosshairTarget;

        private VRTK_Pointer m_Pointer;
        private Vector3 m_StartPos;
        private Quaternion m_StartRot;
        private Vector3 m_Velocity;

        private Vector3 ClosePose => transform.parent.InverseTransformPoint(UnityUtils.CameraTransform.position) + m_CloseOffset;

        private Vector3 TargetPose => m_CrosshairTarget.Distance > 0 ? Vector3.Lerp(ClosePose, m_StartPos, Mathf.Clamp01(m_CrosshairTarget.Distance)) : m_StartPos;

        private void Start() {
            m_Pointer = GetComponentInChildren<VRTK_Pointer>();
            m_StartPos = transform.localPosition;
            m_StartRot = transform.localRotation;
        }

        private void LateUpdate() {
            // if (Globals.IsJournalOpen.Value) {
                if (m_InputSimulator && m_InputSimulator.IsHand) {
                    return;
                }

                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, TargetPose, ref m_Velocity, m_ShiftSpeed, 10, Time.unscaledTime);

                if (m_EnablePointing && m_CrosshairTarget.Target) {
                    transform.LookAt(m_CrosshairTarget.Point);
                    if (m_Pointer) {
                        transform.localRotation = transform.localRotation*Quaternion.Inverse(m_Pointer.transform.localRotation)*Quaternion.Euler(m_RotationOffset);
                    }
                } else {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, m_StartRot, m_ShiftSpeed);
                }
            // }
        }
    }
}