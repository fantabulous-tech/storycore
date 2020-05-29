using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class RotatablePointerByController : MonoBehaviour {
        [SerializeField] private float m_Magnification = 2;

        private VRTK_PointerCursor m_Cursor;
        private Transform m_Pointer;

        private Transform Pointer => UnityUtils.GetOrSet(ref m_Pointer, () => m_Cursor.Pointer ? m_Cursor.Pointer.transform : null);

        private void Start() {
            m_Cursor = GetComponentInParent<VRTK_PointerCursor>();
        }

        private void Update() {
            if (Pointer) {
                Vector3 eulerAngles = Pointer.eulerAngles;
                float rotation = eulerAngles.y - eulerAngles.z*m_Magnification;
                transform.rotation = Quaternion.Euler(0, rotation, 0);
            }
        }
    }
}