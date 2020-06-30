using StoryCore.GameEvents;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTK;

namespace StoryCore.UI {

    public class ToggleMenu : MonoBehaviour {

        [SerializeField] private VRTK_ControllerEvents m_Controller;
        [SerializeField] private VRTK_ControllerEvents.ButtonAlias m_Button;
        [SerializeField] private Vector3 m_PositionOffset = Vector3.forward*0.5f;
        [SerializeField] private Vector3 m_RotationOffset = new Vector3(0, 180, 0);
        [SerializeField] private Vector3 m_PositionOffsetPC = Vector3.forward;
        [SerializeField] private bool m_CanToggleOff;
        [SerializeField, AutoFillAsset(DefaultName = "InVR")] private GameVariableBool m_InVR;
        [SerializeField] private GameVariableBool m_MenuOpen;
        [SerializeField] private GameEventPosAndRot m_MenuLocationEvent;
        [SerializeField] private GameEvent m_ToggleMenu;

        private bool m_Pressed;
        private Transform m_Hmd;

        private Transform Hmd => UnityUtils.GetOrSet(ref m_Hmd, VRTK_DeviceFinder.HeadsetTransform);

        private void Start() {
            if (!m_Controller) {
                Debug.LogError("No Controller assigned.", this);
            }
        }

        protected void OnEnable() {
            m_MenuOpen.Changed += OnMenuOpenChanged;
            m_ToggleMenu.GenericEvent += OnToggleMenu;
        }

        protected void OnDisable() {
            m_MenuOpen.Changed -= OnMenuOpenChanged;
            m_ToggleMenu.GenericEvent -= OnToggleMenu;
        }

        private void LateUpdate() {
            if (!m_Controller) {
                return;
            }

            bool pressed = m_Controller.IsButtonPressed(m_Button);

            if (m_Pressed != pressed && !InputFieldInFocus()) {
                m_Pressed = pressed;
                if (pressed) {
                    OnPress();
                }
            }

            if (pressed) {
                OnHold();
            }
        }

        private static bool InputFieldInFocus() {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            return go && go.TryGetComponent(out InputField _);
        }

        private void OnPress() {
            if (m_CanToggleOff || !m_MenuOpen.Value) {
                StoryDebug.Log($"Toggling journal. OnBefore = {m_MenuOpen.Value}");
                m_ToggleMenu.Raise();
            }
        }

        private void OnHold() {
            if (m_InVR.Value) {
                UpdateJournalLocation();
            }
        }

        private void OnMenuOpenChanged(bool newValue) {
            if (newValue) {
                UpdateJournalLocation();
            }
        }

        private void OnToggleMenu() {
            UpdateJournalLocation();
        }

        private void UpdateJournalLocation() {
            PosAndRot t;
            if (m_InVR.Value) {
                t.position = m_Controller.transform.TransformPoint(m_PositionOffset);
                Quaternion pointAtHmd = Quaternion.LookRotation(transform.position - Hmd.position, Vector3.up);
                t.rotation = pointAtHmd*Quaternion.Euler(m_RotationOffset);
            } else {
                t.position = Hmd.TransformPoint(m_PositionOffsetPC);
                t.rotation = Quaternion.LookRotation(t.position - Hmd.position, Vector3.up);
            }
            m_MenuLocationEvent.Raise(t);
        }
    }
}