using UnityEngine;
using UnityEngine.UI;

namespace StoryCore.UI {
    [RequireComponent(typeof(ScrollRect))]
    public class DisableButtonsOnDrag : MonoBehaviour {
        [SerializeField] private Button m_Blocker;
        [SerializeField] private float m_VelocityThreshold = 0.1f;

        private ScrollRect m_ScrollRect;

        private void Start() {
            m_ScrollRect = GetComponent<ScrollRect>();
            m_ScrollRect.onValueChanged.AddListener(ScrollChanged);
        }

        private void OnDestroy() {
            SetButtonsEnabled(true);
        }

        private void ScrollChanged(Vector2 scroll) {
            SetButtonsEnabled(m_ScrollRect.velocity.magnitude <= m_VelocityThreshold);
        }

        private void SetButtonsEnabled(bool value) {
            m_Blocker.gameObject.SetActive(!value);
        }
    }
}