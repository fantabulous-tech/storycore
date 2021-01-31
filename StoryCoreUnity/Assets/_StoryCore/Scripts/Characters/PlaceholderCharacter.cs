using System.Linq;
using CoreUtils;
using RogoDigital.Lipsync;
using StoryCore.Commands;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Characters {
    public class PlaceholderCharacter : BaseCharacter {
        [SerializeField] private AudioSource m_VoiceSource;

        public override Transform AttentionPoint => m_VoiceSource ? m_VoiceSource.transform : transform;

        public override DelaySequence Perform(ScriptCommandInfo command) {
            // Find the performance name.
            string performanceName = command.Params.GetFirst();

            if (performanceName.IsNullOrEmpty()) {
                Debug.LogWarningFormat(this, "No performance name found.");
                return DelaySequence.Empty;
            }

            // Find the performance object.
            Object performance = Buckets.Performances.Items.FirstOrDefault(i => i && Equals(i.name, performanceName));

            if (performance == null) {
                Debug.LogWarningFormat(this, "Performance '{0}' not found.", performanceName);
                return DelaySequence.Empty;
            }

            return Play(performance);
        }

        public override DelaySequence SetEmotion(ScriptCommandInfo command) {
            Debug.LogWarning($"/emotion not supported on placeholder character {Name}.", this);
            return DelaySequence.Empty;
        }

        public override void LookAt(Transform target) {
            Debug.LogWarning($"/lookAt not supported on placeholder character {Name}.", this);
        }

        private DelaySequence Play(Object performance) {
            if (!gameObject.activeInHierarchy) {
                Debug.LogWarning($"Character {name} disabled. Can't play {performance.name}", this);
                return DelaySequence.Empty;
            }

            CustomPerformance customPerformance = performance as CustomPerformance;

            if (customPerformance != null) {
                return customPerformance.Play(this);
            }

            LipSyncData lipSyncData = performance as LipSyncData;
            if (lipSyncData != null) {
                m_VoiceSource.PlayOneShot(lipSyncData.clip);
                return Delay.For(lipSyncData.clip.length, this);
            }

            // Handle individual animations.
            AnimationClip animClip = performance as AnimationClip;
            if (animClip != null) {
                Debug.LogWarning($"/perform not supported on placeholder character {Name}.", this);
                return DelaySequence.Empty;
            }

            Debug.LogErrorFormat(performance, "Performance type not supported: " + performance.name);
            return DelaySequence.Empty;
        }
    }
}