using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryCore {
    public class MoveOnMouseDown : MonoBehaviour {
        [SerializeField] private float m_MoveHandSpeed = 0.2f;

        private bool m_MouseWasDown;

        private void Update() {
            bool mouseDown = !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0);

            if (mouseDown && !m_MouseWasDown) {
                m_MouseWasDown = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (!mouseDown && m_MouseWasDown) {
                m_MouseWasDown = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (mouseDown) {
                Vector3 delta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
                transform.localPosition += Time.deltaTime*m_MoveHandSpeed*delta;
            }
        }
    }
}