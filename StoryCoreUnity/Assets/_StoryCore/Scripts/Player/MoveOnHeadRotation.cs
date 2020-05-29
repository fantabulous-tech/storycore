using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class MoveOnHeadRotation : MonoBehaviour {
        [SerializeField] private Vector3 m_MinOffset = Vector3.zero;
        [SerializeField] private Vector3 m_MaxOffset = new Vector3(0, 1, 0);
        [SerializeField] private float m_Speed = 1;

        private Vector3 m_StartPos;
        private Transform m_Head;
        private Vector3 m_MinPos;
        private Vector3 m_MaxPos;
        private float m_Progress;
        private Vector3 m_LastForward;
        private Vector3 m_LastMinOffset;
        private Vector3 m_LastMaxOffset;

        private void Start() {
            m_StartPos = transform.localPosition;
            m_MinPos = m_StartPos + m_MinOffset;
            m_MaxPos = m_StartPos + m_MaxOffset;

            if (VRTK_SDKManager.instance.loadedSetup && VRTK_SDKManager.instance.loadedSetup.isValid) {
                SetupHead(VRTK_SDKManager.instance.loadedSetup);
                m_Head = VRTK_SDKManager.instance.loadedSetup.actualHeadset.transform;
            }

            VRTK_SDKManager.instance.LoadedSetupChanged += OnSetupChanged;
        }

        private void SetupHead(VRTK_SDKSetup setup) {
            if (!setup.isValid) {
                m_Head = null;
                m_LastForward = Vector3.forward;
                return;
            }

            m_Head = setup.actualHeadset.transform;
            m_LastForward = m_Head.forward;
        }

        private void OnSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            if (e.currentSetup) {
                m_Head = e.currentSetup.actualHeadset.transform;
            }
        }

        private void FixedUpdate() {
            if (!m_Head) {
                return;
            }

#if UNITY_EDITOR
            if (!m_LastMaxOffset.Approximately(m_MaxOffset) || !m_LastMinOffset.Approximately(m_MinOffset)) {
                m_LastMinOffset = m_MinOffset;
                m_LastMaxOffset = m_MaxOffset;
                m_MinPos = m_StartPos + m_MinOffset;
                m_MaxPos = m_StartPos + m_MaxOffset;
            }
#endif

            Vector3 newForward = m_Head.forward;
            float delta = Vector3.SignedAngle(m_LastForward, newForward, m_Head.right);

            m_Progress = Mathf.Clamp01(m_Progress + delta*m_Speed);
            transform.localPosition = Vector3.Lerp(m_MinPos, m_MaxPos, m_Progress);

            m_LastForward = newForward;
        }
    }
}