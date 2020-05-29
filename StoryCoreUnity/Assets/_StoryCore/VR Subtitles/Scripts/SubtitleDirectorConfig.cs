using UnityEngine;

namespace VRSubtitles {
    [CreateAssetMenu(menuName = "Subtitle Config")]
    public class SubtitleDirectorConfig : ScriptableObject {
        [SerializeField] private SubtitleUI m_Template;

        [Tooltip("Used to calculate the automatic duration of subtitles if no duration is given.")]
        [SerializeField, Range(150, 200)] private int m_WordsPerMinute = 175;

        [SerializeField, Range(0, 10)] private float m_MinimumDuration = 3;
        [SerializeField, Range(0, 5)] private float m_FadeIn = 1;
        [SerializeField] private AnimationCurve m_FadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField, Range(0, 5)] private float m_FadeOut = 1;
        [SerializeField] private AnimationCurve m_FadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public SubtitleUI Template => m_Template;
        public float WPM => m_WordsPerMinute;
        public float MinimumDuration => m_MinimumDuration;
        public float FadeIn => m_FadeIn;
        public AnimationCurve FadeInCurve => m_FadeInCurve;
        public float FadeOut => m_FadeOut;
        public AnimationCurve FadeOutCurve => m_FadeOutCurve;

        [Space, Header("Stay In FOV Settings")]
        [SerializeField] private float m_DistanceFromCamera = 1.3f;

        [SerializeField, Range(1, 10)] private float m_FovMoveSpeed = 2f;

        public float DistanceFromCamera => m_DistanceFromCamera;
        public float FOVMoveSpeed => m_FovMoveSpeed;
    }
}