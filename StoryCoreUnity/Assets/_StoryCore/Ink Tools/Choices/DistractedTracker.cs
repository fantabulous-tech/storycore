using System;
using CoreUtils;
using UnityEngine;
using VRTK;

namespace StoryCore.Choices {
    public class DistractedTracker : MonoBehaviour {
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField, AutoFillAsset] private ChoiceHandler m_DistractedChoice;
        [SerializeField, AutoFillAsset] private ChoiceHandler m_AttentionChoice;
        [SerializeField] private float m_DistractedDelay = 1;
        [SerializeField] private float m_AttentionDelay = 1;

        private DateTime m_LastAttention;
        private DateTime m_LastDistraction;
        private Camera m_PlayerCamera;

        private Camera PlayerCamera => UnityUtils.GetOrSet(ref m_PlayerCamera, () => VRTK_DeviceFinder.HeadsetCamera()?.GetComponent<Camera>());

        private Transform Target => m_StoryTeller.AttentionPoint;
        private bool DistractedChoiceReady => m_DistractedChoice && ChoiceManager.IsValidChoice(m_DistractedChoice);
        private bool AttentionChoiceReady => m_AttentionChoice && ChoiceManager.IsValidChoice(m_AttentionChoice);
        private bool DistractedDelayComplete => DateTime.Now >= m_LastAttention + TimeSpan.FromSeconds(m_DistractedDelay);
        private bool AttentionDelayComplete => DateTime.Now >= m_LastDistraction + TimeSpan.FromSeconds(m_AttentionDelay);
        private bool CanRaisePayingAttention => IsPayingAttention && m_AttentionChoice && AttentionChoiceReady && AttentionDelayComplete;
        private bool CanRaiseDistracted => !IsPayingAttention && !Globals.IsJournalOpen.Value && DistractedChoiceReady && DistractedDelayComplete;

        public bool IsPayingAttention { get; private set; }

        public event Action AttentionChanged;

        private void Start() {
            if (!m_StoryTeller || !m_DistractedChoice || !m_AttentionChoice) {
                Debug.LogErrorFormat(this, "DistractionChoice is missing object references.");
            }

            VRTK_SDKManager.instance.LoadedSetupChanged += OnSetupChanged;
        }

        private void OnSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            m_PlayerCamera = VRTK_DeviceFinder.HeadsetCamera()?.GetComponent<Camera>();
        }

        private void Update() {
            if (!Target) {
                return;
            }

            if (IsOnScreen(Target.position)) {
                m_LastAttention = DateTime.Now;
                if (!IsPayingAttention) {
                    IsPayingAttention = true;
                    AttentionChanged?.Invoke();
                }
            } else {
                m_LastDistraction = DateTime.Now;
                if (IsPayingAttention) {
                    IsPayingAttention = false;
                    AttentionChanged?.Invoke();
                }
            }

            if (CanRaiseDistracted) {
                m_DistractedChoice.Choose();
            } else if (CanRaisePayingAttention) {
                m_AttentionChoice.Choose();
            }
        }

        private bool IsOnScreen(Vector3 position) {
            if (!PlayerCamera) {
                return false;
            }
            Vector3 screenPoint = PlayerCamera.WorldToViewportPoint(position);
            return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0;
        }
    }
}