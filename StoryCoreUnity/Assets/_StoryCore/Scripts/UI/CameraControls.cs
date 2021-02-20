using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryCore {
    [AddComponentMenu("Camera-Control/Unity-Style Camera Control")]
    public class CameraControls : MonoBehaviour {
        [SerializeField] private float m_Speed = 4.0f;
        [SerializeField] private float m_ShiftSpeed = 16.0f;
        [SerializeField] private float m_RotateSpeed = 0.5f;
        [SerializeField] private float m_PanSpeed = 5;
        [SerializeField] private float m_ScrollSpeed = 5;
        [SerializeField] private bool m_ShowInstructions = true;

        private Vector3 m_StartEulerAngles;
        private Vector3 m_StartMousePosition;

        private static bool IsMouseOnScreen {
            get {
                Vector3 viewPoint = UnityUtils.Camera.ScreenToViewportPoint(Input.mousePosition);
                return viewPoint.x > 0 &&
                       viewPoint.x < 1 &&
                       viewPoint.y > 0 &&
                       viewPoint.y < 1 &&
                       viewPoint.z >= 0;
            }
        }

        private void Update() {
            float forward = GetForward();
            float right = GetHorizontal();
            float up = GetVertical();
            float speed = GetSpeed();
            float deltaTime = Time.unscaledDeltaTime;

            // Pan with middle mouse or control.
            if (Input.GetMouseButton(2) || Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftControl)) {
                right -= Input.GetAxis("Mouse X")*m_PanSpeed/2;
                up -= Input.GetAxis("Mouse Y")*m_PanSpeed;
            }

            Vector3 delta = new Vector3(speed*deltaTime*right, up*deltaTime, speed*deltaTime*forward);
            transform.Translate(delta, Space.Self);
            RotateView();
        }

        private float GetSpeed() {
            float currentSpeed = m_Speed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                currentSpeed = m_ShiftSpeed;
            }
            return currentSpeed;
        }

        private static float GetVertical() {
            float up = 0.0f;
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.E)) {
                up += 1.0f;
            }
            if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftShift)) {
                up -= 1.0f;
            }
            return up;
        }

        private static float GetHorizontal() {
            float right = 0.0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                right += 1.0f;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                right -= 1.0f;
            }
            return right;
        }

        private float GetForward() {
            float forward = 0.0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                forward += 1.0f;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                forward -= 1.0f;
            }
            if (!EventSystem.current.IsPointerOverGameObject() && IsMouseOnScreen) {
                forward += Input.GetAxis("Mouse ScrollWheel")*20*m_ScrollSpeed;
            }
            return forward;
        }

        private void RotateView() {
            Vector3 mousePosition = Input.mousePosition;

            // If the right mouse button is first clicked, initialize the starting positions.
            if (Input.GetMouseButtonDown(1)) {
                m_StartMousePosition = mousePosition;
                m_StartEulerAngles = transform.localEulerAngles;
            }

            // If the right mouse button is clicked, rotate the view.
            else if (Input.GetMouseButton(1)) {
                Vector3 offset = (mousePosition - m_StartMousePosition)*m_RotateSpeed;
                transform.localEulerAngles = m_StartEulerAngles + new Vector3(-offset.y*360.0f/Screen.height, offset.x*360.0f/Screen.width, 0.0f);
            }
        }

        //-------------------------------------------------
        private void OnGUI() {
            if (m_ShowInstructions) {
                GUI.Label(new Rect(10.0f, 10.0f, 600.0f, 400.0f),
                          @"WASD/Arrow Keys to translate the camera
Right mouse drag to rotate the camera
Middle mouse drag to pan the camera
Left mouse click for standard interactions");
            }
        }
    }
}