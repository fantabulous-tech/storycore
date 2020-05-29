using UnityEngine;

namespace VRTK {
    public class VRTK_PointerCursor : MonoBehaviour {
        [SerializeField] private GameObject m_FacingObject;

        public Quaternion Facing {
            get {
                if (!m_FacingObject) {
                    m_FacingObject = gameObject;
                }
                return m_FacingObject.transform.rotation;
            }
        }

        public VRTK_BezierPointerRenderer Pointer { get; private set; }

        public void Init(VRTK_BezierPointerRenderer r) {
            Pointer = r;
        }
    }
}