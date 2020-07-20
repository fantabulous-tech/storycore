using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore.Locations {
    public class PlayerLocation : BaseLocation {
        private Vector3 m_Position;
        private Quaternion m_Rotation;
        private Transform m_ActualHmd;
        private Transform m_ActualPlayArea;

        private Transform PlayerHead => UnityUtils.GetOrSet(ref m_ActualHmd, VRTK_DeviceFinder.HeadsetTransform);
        private Transform PlayArea => UnityUtils.GetOrSet(ref m_ActualPlayArea, VRTK_DeviceFinder.PlayAreaTransform);

        protected override Color GizmoColor => Color.green;
        public override Vector3 Position => m_Position;
        public override Quaternion Rotation => m_Rotation;

        private void FixedUpdate() {
            if (PlayerHead != null) {
                Quaternion forward = Quaternion.AngleAxis(PlayerHead.rotation.eulerAngles.y, Vector3.up);
                m_Rotation = Quaternion.Euler(m_RotationOffset.x, PlayerHead.rotation.eulerAngles.y + m_RotationOffset.y, m_RotationOffset.z);
                m_Position = new Vector3(PlayerHead.position.x, PlayArea.position.y, PlayerHead.position.z) + forward*m_PositionOffset;
            }
        }
    }
}