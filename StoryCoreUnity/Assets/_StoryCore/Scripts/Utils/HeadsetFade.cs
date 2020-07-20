using IngameDebugConsole;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore.Commands {
    public class HeadsetFade : Singleton<HeadsetFade> {
        [SerializeField] private float m_Duration = 1;

        private VRTK_HeadsetFade m_Fader;

        private VRTK_HeadsetFade Fader => UnityUtils.GetOrSet(ref m_Fader, this.GetOrAddComponent<VRTK_HeadsetFade>);
        public static float Duration => Instance.m_Duration;

        private DelaySequence m_CurrentFadeSequence;
        
        private void Start() {
            m_Fader = this.GetOrAddComponent<VRTK_HeadsetFade>();

            if (VRTK_SDKManager.instance.loadedSetup == null) {
                VRTK_SDKManager.instance.LoadedSetupChanged += (sender, e) => {
                    if (e.currentSetup != null) {
                        FadeIn();
                    }
                };
            } else {
                FadeIn();
            }
        }

        [ConsoleMethod("fadeDuration", "fadeDuration <duration> - Sets the duration of fade in/out.")]
        private static void SetFadeDuration(float duration) {
            Instance.m_Duration = duration;
        }

        private void CancelFading() {
            if (m_CurrentFadeSequence != null) {
                m_CurrentFadeSequence.Cancel("New Fade Called", this);
            }
        }
        
        public static DelaySequence FadeOut() {
            Instance.Fader.Fade(Color.black, Instance.m_Duration);
            Instance.CancelFading();
            Instance.m_CurrentFadeSequence = Delay.Until(() => {
                return Instance.Fader.IsFaded() && !Instance.Fader.IsTransitioning();
            }, Instance);
            return Instance.m_CurrentFadeSequence;
        }

        public static DelaySequence FadeIn() {
            Instance.Fader.Unfade(Instance.m_Duration);
            Instance.CancelFading();
            return Delay.Until(() => {
                return !Instance.Fader.IsFaded();
            }, Instance);
        }
    }
}