using CoreUtils;
using UnityEngine;
using VRTK;

namespace StoryCore.Utils {
    public class FaceTarget : MonoBehaviour {
        [SerializeField] private Transform m_Target;
        [SerializeField] private bool m_Flip;
        [SerializeField] private bool m_YOnly = true;

        private Transform m_Transform;

        private Transform Target => m_Target;
        private Transform Transform => UnityUtils.GetOrSet(ref m_Transform, () => transform);

        private void Start() {
            if (!m_Target) {
                m_Target = VRTK_DeviceFinder.HeadsetTransform();
                if (VRTK_SDKManager.instance != null) {
                    VRTK_SDKManager.instance.LoadedSetupChanged += OnSetupChange;
                }
            }
        }

        private void OnSetupChange(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            if (e.currentSetup != null && e.currentSetup.actualHeadset != null) {
                m_Target = e.currentSetup.actualHeadset.transform;
            }
        }

        private void LateUpdate() {
            UpdateFacing();
        }

        private void UpdateFacing() {
            if (Target) {
                Vector3 myPos = Transform.position;
                Vector3 targetPos = Target.position;
                Vector3 direction = m_YOnly ? new Vector3(targetPos.x - myPos.x, 0, targetPos.z - myPos.z + 0.0001f) : targetPos - myPos;

                if (direction.sqrMagnitude > 0) {
                    Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    if (m_Flip) {
                        Transform.rotation = Transform.rotation*Quaternion.AngleAxis(180, Transform.up);
                    }
                }
            }
        }
    }
}