using UnityEngine;
using VRTK;

namespace StoryCore {
    public class ControlNonVrPlayer : MonoBehaviour {
        [SerializeField] private SDK_InputSimulator m_InputSimulator;
        [SerializeField] private float m_SpawnPointLeftRightBound = 1;
        [SerializeField] private float m_SpawnPointForwardBackBound = 1;
        [SerializeField] private float m_SpawnPointUpperBound = 0.5f;
        [SerializeField] private float m_SpawnPointLowerBound = -1;

        private Vector3 m_LeftVelocity;
        private Vector3 m_RightVelocity;

        private Vector3 m_LeftQVelocity;
        private Vector3 m_RightQVelocity;

        private void FixedUpdate() {
            //Control override to keep near spawnpoint

            Transform t = m_InputSimulator.m_Player.transform;
            Vector3 pos = t.localPosition;

            if (m_InputSimulator.KeyPressedUp && pos.y < m_SpawnPointUpperBound) {
                t.Translate(Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*Vector3.up);
            }
            if (m_InputSimulator.KeyPressedDown && pos.y > m_SpawnPointLowerBound) {
                t.Translate(Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*Vector3.down);
            }

            pos = t.localPosition;
            Transform parent = t.parent;
            Vector3 right = parent.InverseTransformDirection(t.right);
            Vector3 forward = parent.InverseTransformDirection(t.forward);

            if (m_InputSimulator.KeyPressedForward) {
                pos += Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*forward;
            }
            if (m_InputSimulator.KeyPressedBackward) {
                pos += Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*-forward;
            }
            if (m_InputSimulator.KeyPressedLeft) {
                pos += Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*-right;
            }
            if (m_InputSimulator.KeyPressedRight) {
                pos += Time.fixedDeltaTime*m_InputSimulator.playerMoveMultiplier*right;
            }

            if (pos.z > m_SpawnPointLeftRightBound) {
                pos.z = m_SpawnPointLeftRightBound;
            }
            if (pos.z < -m_SpawnPointLeftRightBound) {
                pos.z = -m_SpawnPointLeftRightBound;
            }
            if (pos.x > m_SpawnPointForwardBackBound) {
                pos.x = m_SpawnPointForwardBackBound;
            }
            if (pos.x < -m_SpawnPointForwardBackBound) {
                pos.x = -m_SpawnPointForwardBackBound;
            }

            t.localPosition = pos;
            // Hand position when posing
        }
    }
}