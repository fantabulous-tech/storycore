using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using VRSubtitles.Utils;
using VRTK;

namespace VRSubtitles {
    public class SubtitleDirector : Singleton<SubtitleDirector> {
        [SerializeField] private SubtitleDirectorConfig m_Config;

        private readonly List<Subtitle> m_SubtitleQueue = new List<Subtitle>();
        private SubtitleUI m_UIInstance;
        private Subtitle m_CurrentSubtitle;
        private Transform m_Player;
        private PlaceInFov m_PlaceInFov;

        private SubtitleUI UIInstance => UnityUtils.GetOrSet(ref m_UIInstance, () => Instantiate(m_Config.Template, transform));
        private bool CanShowNext => m_CurrentSubtitle == null || m_CurrentSubtitle.IsComplete;
        public static SubtitleDirectorConfig Config => UnityUtils.GetOrSet(ref Instance.m_Config, ScriptableObject.CreateInstance<SubtitleDirectorConfig>);

        private void Start() {
            m_Config = m_Config ? m_Config : ScriptableObject.CreateInstance<SubtitleDirectorConfig>();
            m_PlaceInFov = this.GetOrAddComponent<PlaceInFov>();
            m_PlaceInFov.Distance = m_Config.DistanceFromCamera;
            m_PlaceInFov.SmoothTime = m_Config.SmoothTime;
            VRTK_SDKManager.instance.LoadedSetupChanged += OnSetupChanged;
        }

        private void OnSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            CheckForPlayer();
        }

        private void Update() {
            if (m_Player) {
                EvaluateQueue();
            } else {
                CheckForPlayer();
            }
        }

        private void CheckForPlayer() {
            m_Player = VRTK_DeviceFinder.HeadsetTransform();
        }

        private void EvaluateQueue() {
            if (m_SubtitleQueue.Count > 0 && CanShowNext && m_SubtitleQueue[0].TargetTime <= Time.time) {
                Subtitle subtitle = m_CurrentSubtitle = m_SubtitleQueue[0];
                m_SubtitleQueue.RemoveAt(0);
                ShowNow(subtitle);
            }
        }

        private void ShowNow(Subtitle subtitle) {
            m_PlaceInFov.Roost = subtitle.Speaker;
            m_PlaceInFov.SnapToPlace();

            if (subtitle.Template) {
                Transform t = transform;
                t.DestroyAllChildren();
                m_UIInstance = Instantiate(subtitle.Template, t, false);
            }

            UIInstance.Show(subtitle, m_Player);
            subtitle.Show();
            // string duration = subtitle.AutoCloseDuration > 1000000 ? "∞" : subtitle.AutoCloseDuration.ToString("N2");
            // Debug.Log($"[Subtitles] Showing ({duration}) \'{subtitle.Text}\'");
            Delay.For(subtitle.AutoCloseDuration, this).Then(() => FadeOut(subtitle));
        }

        /// <summary>
        ///     Creates a subtitle from the included options and adds it to the subtitle queue.
        /// </summary>
        /// <param name="text">The text of the subtitle.</param>
        /// <param name="portrait">An optional sprite to be used as the speaker's portrait.</param>
        /// <param name="delay">Any delay desired before the text is shown.</param>
        /// <param name="duration">
        ///     The length of time to show the subtitle for. Durations &lt; zero will be automatically
        ///     calculated based on words-per-minute and minimum duration settings.
        /// </param>
        /// <param name="priority">Determines ties when the start times are about the same.</param>
        /// <param name="onComplete">Action to take when the subtitle is complete.</param>
        /// <param name="template">Optional SubtitleUI template to use for this subtitle.</param>
        /// <param name="speaker">The transform on which this subtitle wants to 'roost' on if it's in the FOV.</param>
        /// <returns></returns>
        public static Subtitle Show(string text, Sprite portrait = null, float delay = 0, float duration = -1, int priority = 0, Action onComplete = null, SubtitleUI template = null, Transform speaker = null) {
            Subtitle subtitle = new Subtitle(text, portrait, delay, duration, priority, null, onComplete, template, speaker);
            Instance.m_SubtitleQueue.Add(subtitle);
            Instance.m_SubtitleQueue.Sort(SubtitleComparer.Instance);
            return subtitle;
        }

        /// <summary>
        ///     Creates a subtitle from the included options and adds it to the subtitle queue.
        /// </summary>
        /// <param name="text">The text of the subtitle.</param>
        /// <param name="portrait">An optional sprite to be used as the speaker's portrait.</param>
        /// <param name="delay">Any delay desired before the text is shown.</param>
        /// <param name="duration">
        ///     The length of time to show the subtitle for. Durations &lt; zero will be automatically
        ///     calculated based on words-per-minute and minimum duration settings.
        /// </param>
        /// <param name="priority">Determines ties when the start times are about the same.</param>
        /// <param name="onComplete">Action to take when the subtitle is complete.</param>
        /// <param name="template">Optional SubtitleUI template to use for this subtitle.</param>
        /// <param name="speaker">The transform on which this subtitle wants to 'roost' on if it's in the FOV.</param>
        /// <returns></returns>
        public static Subtitle ShowNow(string text, Sprite portrait = null, float delay = 0, float duration = -1, int priority = 0, Action onComplete = null, SubtitleUI template = null, Transform speaker = null) {
            Clear();
            return Show(text, portrait, delay, duration, priority, onComplete, template, speaker);
        }

        /// <summary>
        ///     Hides the assigned subtitle or removes it from the queue.
        /// </summary>
        /// <param name="subtitle">
        ///     If set, this subtitle will be triggered to hide or removed from the queue.
        ///     If not set, the current subtitle will be hidden.
        /// </param>
        public static DelaySequence FadeOut(Subtitle subtitle = null) {
            if (!AppTracker.IsPlaying) {
                return DelaySequence.Empty;
            }

            subtitle = subtitle ?? Instance.m_CurrentSubtitle;

            if (subtitle == null) {
                return DelaySequence.Empty;
            }

            if (Instance.m_SubtitleQueue.Contains(subtitle)) {
                Instance.m_SubtitleQueue.Remove(subtitle);
                subtitle.Cancel();
                return DelaySequence.Empty;
            }

            return Instance.UIInstance.FadeOut(subtitle).Then(subtitle.Complete);
        }

        /// <summary>
        ///     Hides the current subtitle and clears the subtitle queue.
        /// </summary>
        public static void Clear() {
            if (!Exists) {
                return;
            }

            Instance.m_SubtitleQueue.Clear();
            if (Instance.m_CurrentSubtitle != null) {
                FadeOut(Instance.m_CurrentSubtitle);
                Instance.m_CurrentSubtitle.Cancel();
            }
        }

        private static float WPS => Instance.m_Config.WPM/60;

        public static float GetDuration(string text, float minDuration) {
            // If duration is < 0 (e.g. -1), then calculate the duration based on default words-per-minute.
            // Otherwise, make sure the given duration meets the minimum duration.
            return minDuration < 0 ? GetDuration(text) : Mathf.Max(Instance.m_Config.MinimumDuration, minDuration);
        }

        public static float GetDuration(string text) {
            float min = Instance.m_Config.MinimumDuration;
            return text.IsNullOrEmpty() ? min : Mathf.Max(min, Regex.Matches(text, @"\w+").Count/WPS);
        }
    }
}