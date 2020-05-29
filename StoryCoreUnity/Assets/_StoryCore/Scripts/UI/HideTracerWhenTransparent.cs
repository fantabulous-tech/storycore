using UnityEngine;
using VRTK;

namespace StoryCore {
    [RequireComponent(typeof(VRTK_BasePointerRenderer))]
    public class HideTracerWhenTransparent : MonoBehaviour {
        private VRTK_BasePointerRenderer m_Renderer;

        private void Start() {
            m_Renderer = GetComponent<VRTK_BasePointerRenderer>();
        }

        private void Update() {
            m_Renderer.tracerVisibility = !m_Renderer.IsValidCollision() ? VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff : VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
        }
    }
}