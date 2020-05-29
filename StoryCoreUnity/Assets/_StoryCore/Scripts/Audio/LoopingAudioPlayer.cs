using System;
using System.Linq;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Audio {
    [RequireComponent(typeof(CrossFadeAudioSource))]
    public class LoopingAudioPlayer : MonoBehaviour {
        [SerializeField] private CrossFadeAudioSource m_CrossFader;
        [SerializeField] private AudioBucket m_AudioBucket;
        [SerializeField] private CommandHandler m_PlayCommand;
        [SerializeField] private float m_FadeTime = 1;

        private void OnEnable() {
            m_PlayCommand.Event += OnPlayCommand;
        }

        private void OnDisable() {
            m_PlayCommand.Event -= OnPlayCommand;
        }

        private void OnPlayCommand(string item) {
            Play(item);
        }

        private void Reset() {
            m_CrossFader = this.GetOrAddComponent<CrossFadeAudioSource>();
        }

        public DelaySequence Play(string item) {
            if (item.Equals("off", StringComparison.OrdinalIgnoreCase) || item.Equals("none", StringComparison.OrdinalIgnoreCase)) {
                m_CrossFader.CrossFade(null, m_FadeTime);
                return DelaySequence.Empty;
            }

            AudioClip clip = m_AudioBucket.Items.FirstOrDefault(ac => ac.name.Equals(item, StringComparison.OrdinalIgnoreCase));

            if (clip) {
                m_CrossFader.CrossFade(clip, m_FadeTime);
            } else {
                Debug.LogWarning($"Audio clip '{item}' not found in {m_AudioBucket}.");
            }

            return DelaySequence.Empty;
            // return m_AudioBucket.LoadAudio(item, ac => m_CrossFader.CrossFade(ac, 1, m_FadeTime), Debug.LogWarning);
        }
    }
}