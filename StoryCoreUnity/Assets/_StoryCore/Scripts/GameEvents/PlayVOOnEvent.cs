using RogoDigital.Lipsync;
using StoryCore.AssetBuckets;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameEvents {
    public class PlayVOOnEvent : MonoBehaviour {
        [SerializeField] private string m_VOPrefix;
        [SerializeField] private GameEvent m_Event;
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private float m_PitchMin = 0.97f;
        [SerializeField] private float m_PitchMax = 1.03f;
        [SerializeField, AutoFillAsset] private VOBucket m_VOBucket;

        private void OnEnable() {
            m_Event.GenericEvent += Play;
        }

        private void OnDisable() {
            m_Event.GenericEvent -= Play;
        }

        public void Play() {
            if (!m_AudioSource) {
                Debug.LogErrorFormat(this, "No audio source assigned to {0}", name);
                return;
            }

            m_AudioSource.Stop();

            LipSyncData voData = m_VOBucket.Get(m_VOPrefix);

            if (voData) {
                m_AudioSource.pitch = Random.Range(m_PitchMin, m_PitchMax);
                m_AudioSource.PlayOneShot(voData.clip);
            } else {
                Debug.LogErrorFormat(this, "No VO data found for {0}", m_VOPrefix);
            }
        }
    }
}