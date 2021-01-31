using UnityEngine;

public class CameraController : MonoBehaviour {
    private static float m_Speed = 1.0f;

    private void Update() {
        m_Speed = Mathf.Max(m_Speed += Input.GetAxis("Mouse ScrollWheel"), 0.0f);
        transform.position += (transform.right*Input.GetAxis("Horizontal") + transform.forward*Input.GetAxis("Vertical") + transform.up*Input.GetAxis("Depth"))*m_Speed;
        transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), Input.GetAxis("Rotation"));
    }
}