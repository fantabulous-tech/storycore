using CoreUtils;
using StoryCore;
using StoryCore.HeadGesture;
using StoryCore.Utils;
using UnityEngine;

namespace CoreUtils.GameEvents {
    public class HeadGestureChoice : MonoBehaviour {
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private HeadGestureTracker m_HeadGesture;
        [SerializeField] private BaseGameEvent m_SayYesEvent;
        [SerializeField] private BaseGameEvent m_SayNoEvent;
        [SerializeField] private BaseGameEvent m_YesEvent;
        [SerializeField] private BaseGameEvent m_NoEvent;
        [SerializeField] private float m_ResponseDelay = 1;

        private DelaySequence m_Delay;

        private bool CanRespondYes => (m_Delay == null || m_Delay.IsDone) && m_StoryTeller.IsValidChoice(m_YesEvent);
        private bool CanRespondNo => (m_Delay == null || m_Delay.IsDone) && m_StoryTeller.IsValidChoice(m_NoEvent);

        private void Start() {
            m_HeadGesture.OnYes.AddListener(OnYes);
            m_HeadGesture.OnNo.AddListener(OnNo);

            m_StoryTeller.OnChoicesReady += OnChoicesReady;
            m_StoryTeller.OnChosen += OnChosen;

            m_HeadGesture.enabled = false;
        }

        private void OnChosen() {
            m_HeadGesture.enabled = CanRespondYes || CanRespondNo;
        }

        private void OnChoicesReady() {
            m_HeadGesture.enabled = CanRespondYes || CanRespondNo;
        }

        private void OnYes() {
            if (CanRespondYes) {
                m_SayYesEvent.Raise();
                m_Delay = Delay.For(m_ResponseDelay, this).Then(m_YesEvent.Raise);
            }
        }

        private void OnNo() {
            if (CanRespondNo) {
                m_SayNoEvent.Raise();
                m_Delay = Delay.For(m_ResponseDelay, this).Then(m_NoEvent.Raise);
            }
        }
    }
}