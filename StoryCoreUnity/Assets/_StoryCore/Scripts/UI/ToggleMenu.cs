using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTK;

namespace StoryCore.UI {
    public class ToggleMenu : MonoBehaviour {
        [SerializeField] private Transform m_Menu;
        [SerializeField] private VRTK_ControllerEvents m_Controller;
        [SerializeField] private VRTK_ControllerEvents.ButtonAlias m_Button;
        [SerializeField] private Vector3 m_PositionOffset = Vector3.forward*0.5f;
        [SerializeField] private Vector3 m_RotationOffset = new Vector3(0, 180, 0);
        [SerializeField] private Vector3 m_PositionOffsetPC = Vector3.forward;
        [SerializeField] private bool m_CanToggleOff;
        [SerializeField] private ToggleMenuLocator m_ToggleMenuLocator;

        [SerializeField] private GameVariableBool m_InVR;

        private bool m_Pressed;
        private bool m_On;
        private Transform m_Hmd;

        private Transform Hmd => UnityUtils.GetOrSet(ref m_Hmd, VRTK_DeviceFinder.HeadsetTransform);

        private void Awake() {
            if (!m_ToggleMenuLocator.Value) {
                m_ToggleMenuLocator.Value = this;
            }
        }

        private void Start() {
            StoryDebug.Log($"Journal at start. m_On = {m_On}");
            m_Menu.gameObject.SetActive(m_On);

            if (!m_Menu) {
                Debug.LogError("No Menu Transform assigned.", this);
            }
            if (!m_Controller) {
                Debug.LogError("No Controller assigned.", this);
            }
        }

        private void LateUpdate() {
            if (!m_Menu || !m_Controller) {
                return;
            }

            bool pressed = m_Controller.IsButtonPressed(m_Button);

            if (m_Pressed != pressed && !InputFieldInFocus) {
                m_Pressed = pressed;
                if (pressed) {
                    OnPress();
                }
            }

            if (pressed) {
                OnHold();
            }
        }

        private static bool InputFieldInFocus => EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();

        private void OnPress() {
            m_On = !m_CanToggleOff || !m_Menu.gameObject.activeSelf;
            m_Menu.gameObject.SetActive(m_On);
            StoryDebug.Log($"Toggling journal. m_On = {m_On}");
            UpdateJournalLocation();
        }

        private void OnHold() {
            if (m_InVR.Value) {
                UpdateJournalLocation();
            }
        }

        private void UpdateJournalLocation() {
            if (m_Menu.gameObject.activeSelf) {
                if (m_InVR.Value) {
                    m_Menu.position = m_Controller.transform.TransformPoint(m_PositionOffset);
                    Quaternion pointAtHmd = Quaternion.LookRotation(transform.position - Hmd.position, Vector3.up);
                    m_Menu.rotation = pointAtHmd*Quaternion.Euler(m_RotationOffset);
                } else {
                    m_Menu.position = Hmd.TransformPoint(m_PositionOffsetPC);
                    m_Menu.rotation = Quaternion.LookRotation(m_Menu.position - Hmd.position, Vector3.up);
                }
            }
        }

        public void Open() {
            if (m_On) {
                return;
            }
            m_On = true;
            m_Menu.gameObject.SetActive(m_On);
            StoryDebug.Log($"Showing journal because it's opening. m_On = {m_On}");
            UpdateJournalLocation();
        }

        public void Close() {
            m_On = false;
            m_Menu.gameObject.SetActive(m_On);
            StoryDebug.Log($"Hiding journal because it's closing. m_On = {m_On}");
            OnHold();
        }
    }
}