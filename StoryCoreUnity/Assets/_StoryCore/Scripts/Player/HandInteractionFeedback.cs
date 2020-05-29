using UnityEngine;
using VRTK;

namespace StoryCore.PoseTracking {
    public class HandInteractionFeedback : MonoBehaviour {
        [SerializeField] private Renderer m_Render;
        [SerializeField, Range(0, 1)] private float m_BrightenPercent = 0.2f;

        private VRTK_InteractTouch m_Touch;
        private VRTK_InteractUse m_Use;
        private VRTK_InteractGrab m_Grab;

        private Color m_BaseColor;
        private Color m_BaseGlow;
        private Color m_InteractColor;
        private Color m_InteractGlow;
        private Material m_Material;

        private GameObject m_ObjectTouched;
        private GameObject m_ObjectInUse;

        private bool IsPoking => m_Grab.IsGrabButtonPressed() || m_Use.IsUseButtonPressed();

        private string ColorParam => m_Material.shader.name.Contains("Xray") ? "_OutlineColor" : "_EmissionColor";

        private Color Glow {
            get => m_Material.GetColor(ColorParam);
            set => m_Material.SetColor(ColorParam, value);
        }

        private Color Color {
            get => m_Material.color;
            set => m_Material.color = value;
        }

        private void Start() {
            m_Render = m_Render ? m_Render : GetComponentInChildren<Renderer>();
            m_Material = m_Render.material;

            CalculateColors();

            m_Touch = GetComponentInParent<VRTK_InteractTouch>();
            m_Use = m_Touch.GetComponent<VRTK_InteractUse>();
            m_Grab = m_Touch.GetComponent<VRTK_InteractGrab>();

            m_Touch.ControllerTouchInteractableObject += OnTouch;
            m_Touch.ControllerUntouchInteractableObject += OnUntouch;

            m_Grab.GrabButtonPressed += OnGrab;
            m_Grab.GrabButtonReleased += OnUngrab;

            m_Use.ControllerUseInteractableObject += OnUse;
            m_Use.ControllerUnuseInteractableObject += OnUnuse;
        }

        private void LateUpdate() {
            if (m_Material != m_Render.material) {
                CalculateColors();
            }
        }

        private void CalculateColors() {
            m_Material = m_Render.material;
            m_BaseGlow = Glow;
            m_BaseColor = Color;

            m_InteractGlow = Color.Lerp(Glow, Color.white, m_BrightenPercent);
            m_InteractColor = Color.Lerp(Color, Color.white, m_BrightenPercent);

            UpdateGlow();
        }

        private void OnUse(object sender, ObjectInteractEventArgs e) {
            m_ObjectInUse = e.target;
            UpdateGlow();
        }

        private void OnUnuse(object sender, ObjectInteractEventArgs e) {
            m_ObjectInUse = null;
            UpdateGlow();
        }

        private void OnGrab(object sender, ControllerInteractionEventArgs e) {
            if (m_ObjectInUse) {
                return;
            }
            m_Use.AttemptUse();
            UpdateGlow();
        }

        private void OnUngrab(object sender, ControllerInteractionEventArgs e) {
            m_Use.ForceStopUsing();
            UpdateGlow();
        }

        private void OnTouch(object sender, ObjectInteractEventArgs e) {
            m_ObjectTouched = e.target;
            UpdateGlow();
            if (!m_ObjectInUse && IsPoking) {
                m_Use.AttemptUse();
            }
        }

        private void OnUntouch(object sender, ObjectInteractEventArgs e) {
            m_ObjectTouched = null;
            UpdateGlow();
        }

        private void UpdateGlow() {
            Glow = m_ObjectTouched ? m_InteractGlow : m_BaseGlow;
            Color = m_ObjectTouched ? m_InteractColor : m_BaseColor;
        }
    }
}