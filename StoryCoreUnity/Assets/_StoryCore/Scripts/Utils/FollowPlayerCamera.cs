using UnityEngine;
using VRTK;

namespace StoryCore {
    public class FollowPlayerCamera : MonoBehaviour {
        private Transform m_Target;

        private void LateUpdate() {
            if (m_Target == null || !m_Target.gameObject.activeInHierarchy) {
                m_Target = VRTK_DeviceFinder.HeadsetTransform();
            }
            if (m_Target == null) {
                return;
            }
            transform.SetPositionAndRotation(m_Target.position, m_Target.rotation);
        }
    }
}