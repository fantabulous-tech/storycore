using System;
using CoreUtils;
using CoreUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSubtitles {
    public class SubtitleUI : MonoBehaviour {
        [SerializeField] private FadeCanvasGroup m_Fade;
        [SerializeField] private Image m_Portrait;
        [SerializeField] private TextMeshProUGUI m_Text;

        private Subtitle m_Current;
        private bool m_FadingOut;
        private float m_FadeOutDuration;
        private Vector3 m_InitialScale;
        private bool m_Init;
        private Transform m_Target;

        private FadeCanvasGroup Fade => UnityUtils.GetOrSet(ref m_Fade, this.GetOrAddComponent<FadeCanvasGroup>);
        private Image Portrait => m_Portrait;
        private TextMeshProUGUI Text => UnityUtils.GetOrSet(ref m_Text, GetComponentInChildren<TextMeshProUGUI>);

        private void Start() {
            Init();
        }

        private void Update() {
            CheckForFadeOut();
            UpdateScale();
        }

        private void UpdateScale() {
            if (m_Target != null) {
                transform.localScale = m_InitialScale*Mathf.Clamp(transform.position.DistanceTo(m_Target.position), 1f, 30f);
            }
        }

        private void OnDestroy() {
            Unsubscribe();
        }

        private void Init() {
            if (m_Init) {
                return;
            }
            m_Init = true;
            m_InitialScale = transform.localScale;
        }

        private void CheckForFadeOut() {
            if (m_Current == null) {
                return;
            }

            if (!m_FadingOut && m_Current.TimeRemaining <= m_FadeOutDuration) {
                m_FadingOut = true;
                FadeOut();
            }
        }

        private void Subscribe() {
            if (m_Current != null) {
                m_Current.OnCancel += OnSubtitleCancel;
                m_Current.OnComplete += OnSubtitleComplete;
            }
        }

        private void Unsubscribe() {
            if (m_Current != null) {
                m_Current.OnCancel -= OnSubtitleCancel;
                m_Current.OnComplete -= OnSubtitleComplete;
            }
        }

        private void OnSubtitleComplete() {
            Complete(m_Current);
        }

        private void OnSubtitleCancel() {
            Complete(m_Current);
        }

        public void Show(Subtitle item, Transform target) {
            m_Target = target;
            Init();
            Unsubscribe();
            m_Current = item;
            Subscribe();
            m_FadingOut = false;
            m_FadeOutDuration = Mathf.Min(m_Current.Config.FadeOut, item.AutoCloseDuration*0.4f);

            UpdateScale();

            Fade.In(m_Current.Config.FadeIn, m_Current.Config.FadeInCurve);

            Text.text = item.Text;

            if (Portrait) {
                Portrait.sprite = item.Portrait;
                Portrait.gameObject.SetActive(item.Portrait);
            }
        }

        private DelaySequence FadeOut() {
            Subtitle item = m_Current;
            return Fade.Out(m_FadeOutDuration, m_Current.Config.FadeOutCurve).Then(() => Complete(item));
        }

        public DelaySequence FadeOut(Subtitle item) {
            return !AppTracker.IsPlaying || m_Current != item ? DelaySequence.Empty : FadeOut();
        }

        private void Complete(Subtitle item, Action callback = null) {
            if (m_Current != item || !AppTracker.IsPlaying) {
                return;
            }
 
            m_Current = null;
            m_FadingOut = false;
            callback?.Invoke();
        }
    }
}