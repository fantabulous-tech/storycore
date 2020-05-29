using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Audio {
    public class SoundRandomizer : MonoBehaviour {
        [SerializeField] private AudioSource[] m_AudioSource;
        [SerializeField] private AudioClip[] m_AudioClips;
        [SerializeField] private float m_MinWait = 0.5f;
        [SerializeField] private float m_MaxWait = 2f;

        private float m_NextTime;

        private void OnEnable() {
            SetNextTime();
        }

        private void SetNextTime() {
            m_NextTime = Time.time + Random.Range(m_MinWait, m_MaxWait);
        }

        private void Update() {
            if (Time.time <= m_NextTime) {
                return;
            }

            AudioSource source = m_AudioSource.GetRandomItem();
            AudioClip clip = m_AudioClips.GetRandomItem();
            source.clip = clip;
            source.Play();
            SetNextTime();
        }
    }
}