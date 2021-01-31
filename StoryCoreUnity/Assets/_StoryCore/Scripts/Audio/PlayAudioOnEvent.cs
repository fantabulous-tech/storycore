using StoryCore.Utils;
using UnityEngine;

namespace CoreUtils.GameEvents {
    public class PlayAudioOnEvent : MonoBehaviour {
        [SerializeField] private AudioClip[] m_AudioClips;
        [SerializeField] private GameEvent m_Event;
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private float m_PitchMin = 0.97f;
        [SerializeField] private float m_PitchMax = 1.03f;

        private void OnEnable() {
            m_Event.GenericEvent += OnEvent;
        }

        private void OnDisable() {
            m_Event.GenericEvent -= OnEvent;
        }

        private void OnEvent() {
            m_AudioSource.Stop();
            m_AudioSource.pitch = Random.Range(m_PitchMin, m_PitchMax);
            m_AudioSource.PlayOneShot(m_AudioClips.GetRandomItem());
        }
    }
}