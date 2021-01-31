using CoreUtils;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class FollowHand : MonoBehaviour {
        [SerializeField] private float m_Offset = 0.1f;
        [SerializeField] private GameVariableBool m_DisableGameVarBool;

        private VRTK_ControllerReference m_Target;
        private Transform m_TargetTransform;
        private bool m_FollowEnabled;
        private VRTK_ControllerEvents m_Right;
        private VRTK_ControllerEvents m_Left;

        private Transform TargetTransform => UnityUtils.GetOrSet(ref m_TargetTransform, UpdateHandTarget);

        private void Start() {
            UpdateHandTarget();
            VRTK_SDKManager.SubscribeLoadedSetupChanged(OnSetupChanged);
        }

        private void Update() {
            if (m_DisableGameVarBool != null && m_DisableGameVarBool.Value) {
                return;
            }
            if (TargetTransform && m_FollowEnabled) {
                transform.position = TargetTransform.position + Vector3.up*m_Offset;
            }
        }

        private void OnSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            if (e.currentSetup && e.currentSetup.isValid) {
                UpdateHandTarget();
            }
        }

        private Transform UpdateHandTarget() {
            VRTK_SDKSetup setup = VRTK_SDKManager.GetLoadedSDKSetup();

            if (!setup || !setup.isValid) {
                return null;
            }

            if (m_Right != null) {
                m_Right.TriggerPressed -= OnTriggerPressed;
                m_Right.TriggerReleased -= OnTriggerReleased;
            }
            if (m_Left != null) {
                m_Left.TriggerPressed -= OnTriggerPressed;
                m_Left.TriggerReleased -= OnTriggerReleased;
            }

            VRTK_ControllerReference rightController = VRTK_DeviceFinder.GetControllerReferenceForHand(SDK_BaseController.ControllerHand.Right);
            VRTK_ControllerReference leftController = VRTK_DeviceFinder.GetControllerReferenceForHand(SDK_BaseController.ControllerHand.Left);

            GameObject rightControllerAlias = rightController?.scriptAlias;
            GameObject leftControllerAlias = leftController?.scriptAlias;

            m_Right = rightControllerAlias ? rightControllerAlias.GetComponent<VRTK_ControllerEvents>() : null;
            m_Left = leftControllerAlias ? leftControllerAlias.GetComponent<VRTK_ControllerEvents>() : null;

            if (m_Right != null) {
                m_Right.TriggerPressed += OnTriggerPressed;
                m_Right.TriggerReleased += OnTriggerReleased;
            }
            if (m_Left != null) {
                m_Left.TriggerPressed += OnTriggerPressed;
                m_Left.TriggerReleased += OnTriggerReleased;
            }

            return m_TargetTransform;
        }

        private void OnTriggerReleased(object sender, ControllerInteractionEventArgs e) {
            if (e.controllerReference.Equals(m_Target)) {
                m_FollowEnabled = false;
            }
        }

        private void OnTriggerPressed(object sender, ControllerInteractionEventArgs e) {
            m_Target = e.controllerReference;
            m_TargetTransform = m_Target?.actual ? m_Target?.actual.transform : null;
            m_FollowEnabled = true;
        }
    }
}