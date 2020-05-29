using System;
using System.Linq;
using StoryCore.Utils;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using UnityEngine;

namespace StoryCore.Audio {
    public class EventAudioPlayer : MonoBehaviour {
        [SerializeField] private AudioSource m_Player;
        [SerializeField] private AudioBucket m_AudioBucket;
        [SerializeField] private CommandHandler m_PlayCommand;

        private void OnEnable() {
            m_PlayCommand.Event += OnPlayCommand;
        }

        private void OnDisable() {
            m_PlayCommand.Event -= OnPlayCommand;
        }

        private void OnPlayCommand(string item) {
            Play(item);
        }

        private void Play(string item) {
            if (item.Equals("off", StringComparison.OrdinalIgnoreCase) || item.Equals("none", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            AudioClip clip = m_AudioBucket.Items.FirstOrDefault(ac => ac.name.Equals(item, StringComparison.OrdinalIgnoreCase));
            m_Player.PlayOneShot(clip);
        }
    }
}