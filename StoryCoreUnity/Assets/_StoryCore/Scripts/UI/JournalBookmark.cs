using System;
using CoreUtils;
using DG.Tweening;
using JetBrains.Annotations;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryCore {
    public class JournalBookmark : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private Vector3 m_OnOffset;
        [SerializeField] private Vector3 m_OffOffset = Vector3.up * -30;
        [SerializeField] private float m_Duration = 0.1f;

        [SerializeField, ReadOnly] private bool m_IsOn;

        private Vector3 m_StartPos;
        private bool m_Selected;

        private Vector3 StartPos => m_StartPos;
        private Vector3 UpPos => m_OnOffset + StartPos;
        private Vector3 DownPos => m_OffOffset + StartPos;
        private bool IsUp => IsOn || Selected;

        [UsedImplicitly]
        public bool IsOn {
            get => m_IsOn;
            set {
                if (m_IsOn == value) {
                    return;
                }
                m_IsOn = value;
                UpdateState();
            }
        }
        public bool Selected {
            get => m_Selected;
            set {
                if (m_Selected == value) {
                    return;
                }

                m_Selected = value;
                UpdateState();
            }
        }

        private void Awake() {
            m_StartPos = transform.localPosition;
            UpdateState();
        }

        private void OnEnable() {
            UpdateState();
        }

        private void OnDisable() {
            Selected = false;
            IsOn = false;
        }

        private void UpdateState() {
            transform.DOKill();
            // transform.localPosition = IsOn ? OnPos : OffPos;
            transform.DOLocalMove(IsUp ? UpPos : DownPos, m_Duration).SetUpdate(true).SetEase(Ease.OutSine);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            IsOn = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            IsOn = false;
        }
    }
}