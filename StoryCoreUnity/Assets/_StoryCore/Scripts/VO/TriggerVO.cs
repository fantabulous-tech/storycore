using System;
using CoreUtils;
using RogoDigital.Lipsync;
using StoryCore.Characters;
using StoryCore.Commands;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace StoryCore.Utils {
    public class TriggerVO : MonoBehaviour {
        [SerializeField] private string m_SoundName = "gasp";
        [SerializeField] private Character m_Character;

        [SerializeField, Tooltip("List of VO files to play.")]
        private LipSyncData[] m_VOFiles;

        [SerializeField, Tooltip("Stops the currently playing VO. Otherwise doesn't play.")]
        private bool m_AllowInterrupt;

        [SerializeField, Tooltip("Percentage chance this VO will trigger."), Range(0, 1)]
        private float m_PercentChance = 1;

        [SerializeField, Tooltip("Time to offset playback of VO")]
        private float m_Delay;

        public UnityEvent OnVOTriggered;
        
        private Character Character => UnityUtils.GetOrSet(ref m_Character, GetComponent<Character>);

        public void PlayVOWithDelay(float delayTime) {
            Delay.For(delayTime, this).Then(PlayVO);
        }

        public void PlayVO() {
            PlayVOInternal();
        }

        public void PlayVOType(string nam) {
            if (nam.Equals(m_SoundName, StringComparison.OrdinalIgnoreCase)) {
                PlayVOInternal();
            }
        }

        private void PlayVOInternal() {
            if (!m_Character.isActiveAndEnabled) {
                return;
            }

            if (!m_AllowInterrupt && Character.IsTalking) {
                // Skip VO since character is already talking.
                return;
            }

            if (Random.value > m_PercentChance) {
                // Skip if random value > percentage chance.
                return;
            }

            if (m_Delay > 0) {
                Delay.For(m_Delay, this).Then(PlayVOInternalNow);
            } else {
                PlayVOInternalNow();
            }
        }

        private void PlayVOInternalNow() {
            LipSyncData data = m_VOFiles.GetRandomItem();
            Character.PlayLipSync(data);
            Debug.Log($"Triggering VO: {data.name}.", data);
            OnVOTriggered?.Invoke();
        }

        private void Reset() {
            m_Character = GetComponent<Character>();
        }
    }
}