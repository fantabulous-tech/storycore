using CoreUtils;
using UnityEngine;
using UnityEngine.Serialization;
using VRTK;

namespace VRSubtitles.Utils {
    public class PlaceInFov : MonoBehaviour {
        [SerializeField] private Transform m_Player;
        [SerializeField] private float m_Distance = 1.3f;
        [SerializeField] private float m_VerticalOffset;
        [SerializeField] private bool m_KeepPositionLevel = true;
        [FormerlySerializedAs("m_KeepVertical"), SerializeField] private bool m_KeepRotationVertical;
        [SerializeField] private bool m_UseRotation = true;

        [Header("Roost Settings")]
        [SerializeField] private Transform m_Roost;
        [SerializeField] private Vector3 m_RoostOffset;

        [Header("Keep In FOV Settings")]
        [SerializeField] private bool m_KeepInFov;
        [SerializeField, Range(0, 10)] private float m_SmoothTime = 0.5f;
        [Tooltip("The degrees of rotation off of the center when the follower will start re-centering. 0 = dead center."), SerializeField, Range(0, 90)]
        private float m_RecenterAngle = 60;

        private bool m_RoostCentered;
        private Vector3 m_MoveVelocity;
        private Quaternion m_RotateVelocity;
        private Quaternion m_LastTargetRotation;
        private Vector3 m_LastTargetPosition;

        public float Distance {
            private get => m_Distance;
            set {
                m_Distance = value;
            }
        }
        public float SmoothTime {
            private get => m_SmoothTime;
            set => m_SmoothTime = value;
        }
        
        public Transform Roost {
            set => m_Roost = value;
        }

        private void Update() {
            Transform headsetCamera = VRTK_DeviceFinder.HeadsetCamera();
            if (m_Player != headsetCamera) {
                m_Player = headsetCamera;

                if (m_Player) {
                    SnapToPlace();
                }
            }

            CheckFOV();
        }

        private void CheckFOV() {
            if (!m_KeepInFov) {
                return;
            }

            UpdateFovState(m_Roost, m_Player, ref m_RoostCentered);
            bool isCentered = false;
            UpdateFovState(transform, m_Player, ref isCentered);

            if (!isCentered || m_RoostCentered) {
                m_LastTargetPosition = GetTargetPosition();

                if (m_UseRotation) {
                    m_LastTargetRotation = GetTargetRotation();
                }
            }

            Vector3 position = Vector3.SmoothDamp(transform.position, m_LastTargetPosition, ref m_MoveVelocity, SmoothTime);

            if (m_UseRotation) {
                Quaternion rotation = transform.rotation.SmoothDamp(m_LastTargetRotation, ref m_RotateVelocity, SmoothTime);
                transform.SetPositionAndRotation(position, rotation);
            } else {
                transform.position = position;
            }
        }

        private Vector3 GetForward() {
            if (!m_Player) {
                return Vector3.forward;
            }

            Vector3 forward = m_LastTargetPosition - m_Player.position;
            return m_KeepRotationVertical ? forward.normalized : forward.ZeroY().normalized;
        }

        public void SnapToPlace() {
            if (!m_Player) {
                Debug.LogWarningFormat(this, "Can't snap. No player found.");
                return;
            }

            UpdateFovState(m_Roost, m_Player, ref m_RoostCentered);
            m_LastTargetPosition = GetTargetPosition();

            if (m_UseRotation) {
                m_LastTargetRotation = GetTargetRotation();
            }

            if (m_UseRotation) {
                transform.SetPositionAndRotation(m_LastTargetPosition, m_LastTargetRotation);
            } else {
                transform.position = m_LastTargetPosition;
            }
        }

        private Vector3 GetTargetPosition() {
            if (!m_Player) {
                return Vector3.zero;
            }

            if (m_RoostCentered && m_Roost) {
                return m_Roost.position + m_Player.TransformDirection(m_RoostOffset);
            }

            Vector3 playerFacing = m_Player.forward;
            Vector3 positionOffset = m_KeepPositionLevel ? playerFacing.ZeroY().normalized : playerFacing;
            Vector3 up = m_KeepPositionLevel ? Vector3.up : m_Player.up;
            return m_Player.position + positionOffset*Distance + up*m_VerticalOffset;
        }

        private Quaternion GetTargetRotation() {
            return !m_Player ? Quaternion.identity : Quaternion.LookRotation(GetForward());
        }

        private void UpdateFovState(Transform target, Transform player, ref bool wasCentered) {
            if (!target || !player) {
                wasCentered = false;
                return;
            }

            float angle = Vector3.Angle(target.position - player.position, player.forward);
            if (wasCentered && !InCenter(angle)) {
                wasCentered = false;
            } else if (!wasCentered && InCenter(angle, 0.5f)) {
                wasCentered = true;
            }
        }

        private bool InCenter(float angle, float scale = 1) {
            return angle < m_RecenterAngle*scale;
        }

        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) {
                return;
            }

            Vector3 targetPosition = GetTargetPosition();
            Gizmos.DrawSphere(targetPosition, 0.02f);
            Gizmos.DrawRay(targetPosition, GetForward()*0.1f);
        }
    }
}