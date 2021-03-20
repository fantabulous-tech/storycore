using StoryCore;
using StoryCore.Choices;
using StoryCore.HeadGesture;
using UnityEngine;

namespace CoreUtils.GameEvents {
    public class HeadGestureChoiceTracker : MonoBehaviour {
        [SerializeField, AutoFillFromScene] private StoryTeller m_StoryTeller;
        [SerializeField] private HeadGestureTracker m_HeadGesture;
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

        private void OnDestroy() {
            if (m_HeadGesture) {
                m_HeadGesture.OnYes.RemoveListener(OnYes);
                m_HeadGesture.OnNo.RemoveListener(OnNo);
            }

            if (m_StoryTeller) {
                m_StoryTeller.OnChoicesReady -= OnChoicesReady;
                m_StoryTeller.OnChosen -= OnChosen;
            }
        }

        private void OnChosen() {
            m_HeadGesture.enabled = CanRespondYes || CanRespondNo;
            m_Delay?.Cancel("A choice was chosen.", this);
        }

        private void OnChoicesReady() {
            m_HeadGesture.enabled = CanRespondYes || CanRespondNo;
        }

        private void OnYes() {
            if (!CanRespondYes) {
                return;
            }

            m_SayYesEvent.Raise();
            m_Delay = Delay.For(m_ResponseDelay, this).Then(() => {
                if (m_YesChoice) {
                    m_YesChoice.Choose();
                }
            });
        }

        private void OnNo() {
            if (!CanRespondNo) {
                return;
            }

            m_SayNoEvent.Raise();
            m_Delay = Delay.For(m_ResponseDelay, this).Then(() => {
                if (m_NoChoice) {
                    m_NoChoice.Choose();
                }
            });
        }
    }
}