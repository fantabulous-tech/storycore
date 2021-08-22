using System;
using System.Linq;
using System.Text.RegularExpressions;
using CoreUtils;
using Ink.Runtime;
using Polyglot;
using RogoDigital.Lipsync;
using UnityEngine;

namespace StoryCore {

    public class VODialogLineSequence : DialogLineSequence, ILocalize {
        protected readonly string m_OriginalText;
        private readonly string m_LineId;
        private readonly Regex m_NumberRegex = new Regex(@"^[0-9]+$");

        private LipSyncData m_Clip;
        private bool m_ClipSearched;
        private string m_LocalizationKey;
        private IPerformLipSync m_Character;

        protected override bool UseSubtitles => !Clip || Localization.Instance.SelectedLanguage != Language.English || m_StoryTeller.UseSubtitles && base.UseSubtitles;

        private LipSyncData Clip {
            get {
                if (m_ClipSearched) {
                    return m_Clip;
                }

                string section = m_Section.IsNullOrEmpty() ? "" : m_Section + ".";
                m_Clip = m_LineId.IsNullOrEmpty() ? null : GetVO(m_LineId);
                m_LocalizationKey = m_Clip ? section + m_Clip.name.ReplaceRegex("-[0-9]+$", "") : m_LineId;
                Localization.Instance.AddOnLocalizeEvent(this);
                m_ClipSearched = true;
                return m_Clip;
            }
        }
        
        public VODialogLineSequence(StoryTeller storyTeller, string text, string section) : base(storyTeller, text, section) {
            Story story = m_StoryTeller.Story;
            m_OriginalText = text;
            m_Text = text;

            if (story.currentTags.Count == 0) {
                Debug.LogWarning($"Untagged line found in {section}: {m_OriginalText}");
                return;
            }

            string prefix = story.currentTags.FirstOrDefault(t => m_NumberRegex.IsMatch(t));

            // Force '1' -> '01', etc.
            if (!prefix.IsNullOrEmpty() && prefix.Length == 1 && Char.IsDigit(prefix[0])) {
                prefix = "0" + prefix;
            }

            if (!section.IsNullOrEmpty()) {
                section += ".";
            }

            m_LineId = section + prefix;
        }

        protected virtual LipSyncData GetVO(string lineId) {
            return Globals.VO.Get(lineId);
        }

        ~VODialogLineSequence() {
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        public override void Start() {
            base.Start();

            if (Clip != null) {
                Play(Clip, m_Section);
            } else {
                if (m_StoryTeller.FocusedCharacter is IPerformLipSync lipSyncCharacter) {
                    lipSyncCharacter.StopLipSync();
                }

                Debug.LogWarning($"No lipsync VO file found for {m_LineId}: {m_Text}");
            }
        }

        protected override float GetDuration() {
            return Clip != null ? Clip.length + GetPunctuationPause(m_OriginalText) : base.GetDuration();
        }

        public override void Interrupt() {
            base.Interrupt();
            if (Clip != null) {
                Stop(Clip, m_Section);
            }
        }

        protected override void OnComplete() {
            if (Clip == null) {
                base.OnComplete();
                return;
            }

            // Log("Audio clip '" + Clip.name + "' duration complete.", Clip);
            IsComplete = true;
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        private void Play(LipSyncData clip, string section) {
            StoryDebug.Log($"PLAYING VO: {section}.{clip.name}: \"{m_OriginalText}\"", clip);

            if (m_StoryTeller.FocusedCharacter is IPerformLipSync lipSyncCharacter) {
                m_Character = lipSyncCharacter;
                lipSyncCharacter.PlayLipSync(clip);
            } else {
                Debug.LogWarning("No audio source found for VO. Playing from camera's position.");
                clip.clip.PlayOneShot();
            }
        }

        private void Stop(LipSyncData clip, string section) {
            StoryDebug.Log($"STOPPING VO: {section}.{(clip ? clip.name : "null")}", clip);

            if (m_Character != null) {
                m_Character.StopLipSync();
            } else if (clip && clip.clip) {
                clip.clip.StopOneShot();
            }

            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        protected virtual string GetLocalizedText(string key) {
            return Localization.Get(key);
        }

        public void OnLocalize() {
            if (!Application.isPlaying) {
                Debug.LogWarning($"Removing old localization event: {m_OriginalText}");
                Localization.Instance.RemoveOnLocalizeEvent(this);
                return;
            }

            string localizationText = GetLocalizedText(m_LocalizationKey); // text;

            if (!Globals.Exists || !Localization.KeyExist(m_LocalizationKey) || localizationText.IsNullOrEmpty()) {
                Debug.LogWarning($"Could not find localization text for {m_LocalizationKey}: {m_OriginalText}");
                m_Text = m_OriginalText;
            } else {
                m_Text = Globals.TextReplacementConfig.Convert(localizationText);
            }
        }

        private static float GetPunctuationPause(string text) {
            return text.IsNullOrEmpty() ? 0 : GetPunctuationPause(text.Trim().Last());
        }

        private static float GetPunctuationPause(char c) {
            // If the last character was a letter, then the break is
            // a continuous sentence and shouldn't have any pause.
            if (Char.IsLetter(c)) {
                return 0;
            }

            // Otherwise, use longer pause for special 'pause' punctuation
            // or a short pause otherwise.
            switch (c) {
                case '.':
                case '?':
                case '"':
                case '\'':
                case '\\':
                case '~':
                case ';':
                case ':':
                case ')':
                case ']':
                    return 0.8f;
                case '-':
                case '/':
                    return 0;
                default:
                    return 0.4f;
            }
        }

        public override string ToString() {
            return $"{base.ToString()} ({m_LineId})";
        }
    }
}