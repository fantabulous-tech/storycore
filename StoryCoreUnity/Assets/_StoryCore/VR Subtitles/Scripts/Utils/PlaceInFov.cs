using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace VRSubtitles.Utils {
    public class PlaceInFov : MonoBehaviour {
        [SerializeField] private Transform m_Player;
        [SerializeField] private float m_Distance = 1.3f;
        [SerializeField] private float m_VerticalOffset;
        [SerializeField] private Transform m_Roost;
        [SerializeField] private Vector3 m_RoostOffset;

        [Tooltip("The degrees of rotation off of the center when the follower will start re-centering. 0 = dead center."), SerializeField, Range(0, 90)]
        private float m_RecenterAngle = 40;

        [Header("Keep In FOV Settings")]
        [SerializeField] private bool m_KeepInFov;

        [SerializeField, Range(1, 10)] private float m_SmoothTime = 2f;
        [SerializeField, Range(1, 10)] private float m_Speed = 2f;

        private bool m_SubtitleCentered;
        private bool m_RoostCentered;
        private Vector3 m_LastTargetPosition;
        private Transform m_LastPlayer;
        private Vector3 m_Velocity;

        public float Distance {
            private get => m_Distance;
            set {
                m_Distance = value;
            }
        }
        public float Speed {
            private get => m_Speed;
            set => m_Speed = value;
        }
        public float RecenterAngle {
            private get => m_RecenterAngle;
            set => m_RecenterAngle = value;
        }
        public Transform Roost {
            set => m_Roost = value;
        }

        private Vector3 TargetPosition {
            get {
                Transform t = m_Player ? m_Player : VRTK_DeviceFinder.HeadsetCamera();
                if (t == null) {
                    t = UnityUtils.CameraTransform;
                }
                if (m_RoostCentered && m_Roost) {
                    return m_Roost.position + t.TransformDirection(m_RoostOffset);
                }
                return m_LastTargetPosition = !t ? Vector3.zero : t.position + t.forward.ZeroY().normalized*Distance + Vector3.up*m_VerticalOffset;
            }
        }

        private void Update() {
            Transform headsetCamera = VRTK_DeviceFinder.HeadsetCamera();
            if (m_Player != headsetCamera) {
                m_Player = headsetCamera;

                if (m_Player) {
                    SnapToTargetPosition();
                }
            }

            if (m_KeepInFov) {
                transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref m_Velocity, m_SmoothTime);
            }
        }

        public void SnapToTargetPosition() {
            if (!m_Player) {
                Debug.LogWarningFormat(this, "Can't snap. No player found.");
                return;
            }

            UpdateFovState(m_Roost, m_Player, ref m_RoostCentered);
            //UpdateFovState(transform, ref m_SubtitleCentered);
            transform.position = TargetPosition;
        }

        private void UpdateFovState(Transform target, Transform player, ref bool wasCentered) {
            if (!target) {
                wasCentered = false;
                return;
            }

            float angle = Vector3.Angle(target.position - player.position, player.forward);
            if (!InCenter(angle) && wasCentered) {
                wasCentered = false;
            } else if (InCenter(angle, 0.5f) && !wasCentered) {
                wasCentered = true;
            }
        }

        private bool InCenter(float angle, float scale = 1) {
            return angle < RecenterAngle*scale;
        }

        private void OnDrawGizmosSelected() {
            if (Application.isPlaying) {
                Gizmos.DrawSphere(TargetPosition, 0.02f);
            }
        }
    }
}