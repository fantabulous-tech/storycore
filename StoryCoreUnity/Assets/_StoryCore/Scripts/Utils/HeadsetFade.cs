using CoreUtils;
using IngameDebugConsole;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore.Commands {
    public class HeadsetFade : Singleton<HeadsetFade> {
        [SerializeField] private float m_Duration = 1;

        private VRTK_HeadsetFade m_Fader;

        private VRTK_HeadsetFade Fader => UnityUtils.GetOrSet(ref m_Fader, this.GetOrAddComponent<VRTK_HeadsetFade>);
        public static float Duration => Exists ? Instance.m_Duration : 0;

        private void Start() {
            m_Fader = this.GetOrAddComponent<VRTK_HeadsetFade>();
            FadeIn();
        }

        [ConsoleMethod("fadeDuration", "fadeDuration <duration> - Sets the duration of fade in/out.")]
        private static void SetFadeDuration(float duration) {
            if (Exists) {
                Instance.m_Duration = duration;
            } else {
                Debug.LogWarning("Couldn't find HeadsetFade instance.");
            }
        }

        public static DelaySequence FadeOut() {
            if (!Exists) {
                Debug.LogWarning("Couldn't find HeadsetFade instance.");
                return DelaySequence.Empty;
            }

            Instance.Fader.Fade(Color.black, Instance.m_Duration);
            return Delay.Until(() => !Exists || Instance && Instance.Fader && Instance.Fader.IsFaded() && !Instance.Fader.IsTransitioning(), Instance);
        }

        public static DelaySequence FadeIn() {
            if (!Exists) {
                Debug.LogWarning("Couldn't find HeadsetFade instance.");
                return DelaySequence.Empty;
            }

            Instance.Fader.Unfade(Instance.m_Duration);
            return Delay.Until(() => Instance && !Instance.Fader.IsFaded(), Instance);
        }
    }
}