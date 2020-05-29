using System.Linq;
using RogoDigital.Lipsync;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu]
    public class CustomPerformance : ScriptableObject {
        [SerializeField] private AnimationClip[] m_AnimationClips;
        [SerializeField] private float m_AnimTransition = -1;
        [SerializeField] private LipSyncData[] m_LipSyncClips;
        [SerializeField] private AudioClip[] m_AudioClips;
        [SerializeField] private float m_AudioDelay;
        [SerializeField] private GameEvent m_Event;
        [SerializeField] private float m_EventDelay;
        [SerializeField] private AnimationCurve m_LookAtWeight = AnimationCurve.Constant(0, 1, 1);

        public bool HasClip => m_AnimationClips.Length > 0;
        public AnimationClip[] Clips => m_AnimationClips;

        public DelaySequence Play(BaseCharacter character) {
            DelaySequence animSequence = TryPlayAnimation(character);
            DelaySequence audioSequence = TryPlayLipSync(character);

            if (audioSequence == null) {
                audioSequence = TryPlayAudio(character);
            }

            DelaySequence eventSequence = TryTriggerEvent();

            return Delay.Until(() => SequencesComplete(animSequence, audioSequence, eventSequence), this);
        }

        private static bool SequencesComplete(params DelaySequence[] sequences) {
            return sequences.All(SequenceComplete);
        }

        private static bool SequenceComplete(DelaySequence sequence) {
            return sequence == null || sequence.IsDone;
        }

        private DelaySequence TryTriggerEvent() {
            if (!m_Event) {
                return null;
            }

            if (m_EventDelay >= 0) {
                return Delay.For(m_EventDelay, this).Then(() => m_Event.Raise());
            }

            m_Event.Raise();
            return DelaySequence.Empty;
        }

        private DelaySequence TryPlayAnimation(BaseCharacter character) {
            AnimationClip clip = m_AnimationClips.GetRandomItem();
            if (clip != null && character is IPerformAnim animCharacter) {
                return animCharacter.PlayAnim(clip, m_AnimTransition, -1, m_LookAtWeight);
            }
            return null;
        }

        private DelaySequence TryPlayLipSync(BaseCharacter character) {
            LipSyncData clip = m_LipSyncClips.GetRandomItem();
            if (clip == null) {
                return null;
            }

            if (character is IPerformLipSync lipSyncCharacter) {
                lipSyncCharacter.PlayLipSync(clip, m_AudioDelay);
            }

            return null;
        }

        private DelaySequence TryPlayAudio(BaseCharacter character) {
            return TryPlayAudio(character, m_AudioClips.GetRandomItem());
        }

        private DelaySequence TryPlayAudio(BaseCharacter character, AudioClip clip) {
            if (clip == null) {
                return null;
            }

            if (character is IPerformAudio audioCharacter) {
                return audioCharacter.PlayAudio(clip, m_AudioDelay);
            }

            return PlayAudioWithoutCharacter(clip);
        }

        private DelaySequence PlayAudioWithoutCharacter(AudioClip clip) {
            Debug.LogWarningFormat(this, "No character found for audio. Playing from camera's position.");
            AudioSource.PlayClipAtPoint(clip, UnityUtils.CameraTransform.position, 0.5f);
            return Delay.For(clip.length, this);
        }
    }
}