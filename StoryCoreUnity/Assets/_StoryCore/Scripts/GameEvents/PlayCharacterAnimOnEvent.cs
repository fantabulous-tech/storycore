using StoryCore.Characters;
using UnityEngine;

namespace StoryCore.GameEvents {
    public class PlayCharacterAnimOnEvent : MonoBehaviour {
        [SerializeField] private Character m_Character;
        [SerializeField] private AnimationClip m_Clip;
        [SerializeField] private GameEvent m_Event;
        [SerializeField] private float m_TransitionDuration = -1;

        private void OnEnable() {
            m_Event.GenericEvent += OnEvent;
        }

        private void OnDisable() {
            m_Event.GenericEvent -= OnEvent;
        }

        private void OnEvent() {
            m_Character.PlayAnim(m_Clip, 0, m_TransitionDuration);
        }
    }
}