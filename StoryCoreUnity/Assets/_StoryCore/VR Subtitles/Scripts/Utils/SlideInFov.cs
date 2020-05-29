using UnityEngine;
using VRTK;

namespace VRSubtitles.Utils {
    public class SlideInFov : MonoBehaviour {
        [SerializeField] private Transform m_Player;
        [SerializeField] private float m_Distance = 1.3f;
        [SerializeField, Range(1, 10)] private float m_Speed = 2f;
        [SerializeField] private bool m_SlideToRoost;
        [SerializeField] private AnimationCurve m_SlideCurve;

        [Tooltip("The degrees of rotation off of the center when the follower will start re-centering. 0 = dead center."), SerializeField, Range(0, 90)]
        private float m_RecenterAngle = 40;

        [SerializeField] private Transform m_Roost;
        [SerializeField] private Vector3 m_RoostOffset;

        public float Distance {
            private get {
                return m_Distance;
            }
            set {
                m_Distance = value;
            }
        }
        public float Speed {
            private get {
                return m_Speed;
            }
            set {
                m_Speed = value;
            }
        }
        public float RecenterAngle {
            private get {
                return m_RecenterAngle;
            }
            set {
                m_RecenterAngle = value;
            }
        }

        private bool m_SubtitleCentered;
        private bool m_RoostCentered;
        private Vector3 m_LastTargetPosition;

        public Transform Roost {
            set => m_Roost = value;
        }

        private Vector3 TargetPosition {
            get {
                if (m_RoostCentered) {
                    return m_Roost.position + m_Player.localRotation*m_RoostOffset;
                }
                if (m_SubtitleCentered) {
                    return m_LastTargetPosition;
                }
                return m_LastTargetPosition = m_Player.position + new Vector3(m_Player.forward.x, 0, m_Player.forward.z).normalized*Distance;
            }
        }

        private void Start() {
            m_Player = VRTK_DeviceFinder.HeadsetTransform();
            if (!m_Player) {
                VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;
            } else {
                SnapToTargetPosition();
            }
        }

        private void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            m_Player = VRTK_DeviceFinder.HeadsetTransform();
            SnapToTargetPosition();
        }

        public void SnapToTargetPosition() {
            if (!m_Player) {
                return;
            }

            UpdateFovState(m_Roost, ref m_RoostCentered);
            UpdateFovState(transform, ref m_SubtitleCentered);

            transform.position = TargetPosition;
        }

        private void UpdateFovState(Transform target, ref bool wasCentered) {
            if (!target) {
                wasCentered = false;
                return;
            }

            float angle = Vector3.Angle(target.position - m_Player.position, m_Player.forward);
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