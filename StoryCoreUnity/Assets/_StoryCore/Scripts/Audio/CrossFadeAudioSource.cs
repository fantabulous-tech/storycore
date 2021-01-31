using StoryCore.Utils;
using DG.Tweening;
using CoreUtils.GameVariables;
using UnityEngine;

namespace StoryCore.Audio {
    [ExecuteInEditMode]
    public class CrossFadeAudioSource : MonoBehaviour {
        [SerializeField] private AudioSource m_Source0;
        [SerializeField] private AudioSource m_Source1;
        [SerializeField] private GameVariableBool m_EnabledSetting;
        [SerializeField] private float m_DefaultFadeDuration = 1;

        private AudioSource m_CurrentSource;
        private Sequence m_Sequence;

        private bool HasSources => m_Source0 != null && m_Source1 != null;
        public bool IsPlaying => HasSources && (m_Source0.isPlaying || m_Source1.isPlaying);

        private void Awake() {
            CheckAudioSources();

            if (m_EnabledSetting != null) {
                m_EnabledSetting.Changed += OnEnabledSettingChanged;
                OnEnabledSettingChanged(m_EnabledSetting.Value);
            }
        }

        private void OnEnabledSettingChanged(bool enable) {
            m_Source0.mute = !enable;
            m_Source1.mute = !enable;
        }

        private void Reset() {
            CheckAudioSources();
        }

        private void CheckAudioSources() {
            if (!HasSources) {
                InitAudioSources();
            }
        }

        private void InitAudioSources() {
            AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
            m_Source0 = audioSources.Length > 0 ? audioSources[0] : gameObject.AddComponent<AudioSource>();
            m_Source1 = audioSources.Length > 1 ? audioSources[1] : gameObject.AddComponent<AudioSource>();
        }

        public void CrossFade(AudioClip clip, float maxVolume, float fadingTime = -1) {
            AudioSource currentSource = m_CurrentSource == m_Source0 ? m_Source0 : m_Source1;
            AudioSource nextSource = m_CurrentSource == m_Source0 ? m_Source1 : m_Source0;
            fadingTime = fadingTime < 0 ? m_DefaultFadeDuration : fadingTime;

            // If we are fading to the same clip, then skip fading.
            if (clip && m_CurrentSource && m_CurrentSource.isPlaying && clip == m_CurrentSource.clip) {
                return;
            }

            if (clip) {
                nextSource.clip = clip;
                nextSource.Play();
            } else {
                nextSource.Stop();
            }

            nextSource.volume = 0;

            if (m_Sequence != null && m_Sequence.active) {
                m_Sequence.Kill();
            }

            m_Sequence = DOTween.Sequence();
            m_Sequence.Append(currentSource.DOFade(0, fadingTime));

            if (clip) {
                m_Sequence.Join(nextSource.DOFade(maxVolume, fadingTime));
            }

            m_CurrentSource = nextSource;
        }
    }
}