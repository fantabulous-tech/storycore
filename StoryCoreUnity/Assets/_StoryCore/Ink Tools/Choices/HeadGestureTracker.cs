using CoreUtils;
using StoryCore;
using StoryCore.Choices;
using StoryCore.HeadGesture;
using StoryCore.Utils;
using UnityEngine;

namespace CoreUtils.GameEvents {
    public class HeadGestureTracker : MonoBehaviour {
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private StoryCore.HeadGesture.HeadGestureTracker m_HeadGesture;
        [SerializeField, AutoFillAsset] private BaseGameEvent m_SayYesEvent;
        [SerializeField, AutoFillAsset] private BaseGameEvent m_SayNoEvent;
        [SerializeField, AutoFillAsset] private ChoiceHandler m_YesChoice;
        [SerializeField, AutoFillAsset] private ChoiceHandler m_NoChoice;
        [SerializeField] private float m_ResponseDelay = 1;

        private DelaySequence m_Delay;

        private bool CanRespondYes => (m_Delay == null || m_Delay.IsDone) && ChoiceManager.IsValidChoice(m_YesChoice);
        private bool CanRespondNo => (m_Delay == null || m_Delay.IsDone) && ChoiceManager.IsValidChoice(m_NoChoice);

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
                m_Delay = Delay.For(m_ResponseDelay, this).Then(m_YesChoice.Choose);
            }
        }

        private void OnNo() {
            if (CanRespondNo) {
                m_SayNoEvent.Raise();
                m_Delay = Delay.For(m_ResponseDelay, this).Then(m_NoChoice.Choose);
            }
        }
    }
}