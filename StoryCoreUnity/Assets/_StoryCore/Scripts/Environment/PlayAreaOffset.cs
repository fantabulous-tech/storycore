using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class PlayAreaOffset : MonoBehaviour {
        [SerializeField] private Transform m_HmdDirectionTarget;

        private Transform m_ActualHmd;
        private Transform m_ActualPlayArea;

        private Transform ActualHmd => UnityUtils.GetOrSet(ref m_ActualHmd, VRTK_DeviceFinder.HeadsetTransform);
        private Transform ActualPlayArea => UnityUtils.GetOrSet(ref m_ActualPlayArea, VRTK_DeviceFinder.PlayAreaTransform);

        private void LateUpdate() {
            if (!m_HmdDirectionTarget || !ActualHmd || !ActualPlayArea) {
                return;
            }

            Quaternion rotationOffset = Quaternion.AngleAxis(ActualPlayArea.eulerAngles.y - ActualHmd.eulerAngles.y, Vector3.up);
            transform.rotation = m_HmdDirectionTarget.rotation*rotationOffset;

            Vector3 positionOffset = ActualPlayArea.InverseTransformPoint(ActualHmd.position);
            transform.position = m_HmdDirectionTarget.TransformPoint(new Vector3(-positionOffset.x, 0, -positionOffset.z));
        }
    }
}