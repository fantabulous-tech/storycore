using System.Linq;
using CoreUtils;
using CoreUtils.GameEvents;
using RogoDigital.Lipsync;
using UnityEngine;

namespace StoryCore.Characters {
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
        public AnimationClip ChosenAnim { get; private set; }

        public DelaySequence Play(BaseCharacter character) {
            ChosenAnim = null;
            DelaySequence animSequence = TryPlayAnimation(character);
            DelaySequence audioSequence = TryPlayLipSync(character) ?? TryPlayAudio(character);
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
            ChosenAnim = m_AnimationClips.GetRandomItem();
            if (ChosenAnim != null && character is IPerformAnim animCharacter) {
                return animCharacter.PlayAnim(ChosenAnim, m_AnimTransition, -1, AnimUpdate);
            }
            return null;
        }

        protected virtual void AnimUpdate(object sender, AnimationClip clip, float progress, float weight) {
            if (weight > 0.5f && Clips.Contains(clip) && sender is Character character) {
                UpdateLookAt(character, progress);
            }
        }

        private void UpdateLookAt(Character character, float progress) {
            if (m_LookAtWeight != null) {
                character.SetLookWeight(m_LookAtWeight.Evaluate(progress));
            } else {
                character.SetLookWeight(1);
            }
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