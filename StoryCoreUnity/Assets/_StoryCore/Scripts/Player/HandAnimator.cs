using UnityEngine;
using VRTK;

namespace StoryCore.Animation {
    public class HandAnimator : MonoBehaviour {
        [SerializeField] private Animator m_Animator;

        private VRTK_ControllerEvents m_Events;
        private static readonly int s_Poke = Animator.StringToHash("Poke");
        private static readonly int s_Down = Animator.StringToHash("Down");

        private void Start() {
            m_Animator = m_Animator ? m_Animator : GetComponent<Animator>();

            m_Events = GetComponentInParent<VRTK_ControllerEvents>();
            m_Events.GripPressed += UpdatePoke;
            m_Events.GripReleased += UpdatePoke;
            m_Events.TriggerHairlineStart += UpdatePoke;
            m_Events.TriggerHairlineEnd += UpdatePoke;
            m_Events.TriggerPressed += OnTriggerPressed;
            m_Events.TriggerReleased += OnTriggerReleased;
        }

        private void UpdatePoke(object sender, ControllerInteractionEventArgs e) {
            m_Animator.SetBool(s_Poke, m_Events.gripPressed || m_Events.triggerHairlinePressed);
        }

        private void OnTriggerPressed(object sender, ControllerInteractionEventArgs e) {
            m_Animator.SetBool(s_Down, true);
        }

        private void OnTriggerReleased(object sender, ControllerInteractionEventArgs e) {
            m_Animator.SetBool(s_Down, false);
        }
    }
}