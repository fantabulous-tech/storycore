using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ink.Runtime;
using RogoDigital.Lipsync;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class VODialogLineSequence : DialogLineSequence {
        private readonly LipSyncData m_Clip;
        private readonly string m_LineId;

        private IPerformLipSync m_Character;

        protected override bool UseSubtitles => !m_Clip || m_StoryTeller.UseSubtitles && base.UseSubtitles;
        private readonly Regex m_NumberRegex = new Regex(@"^[0-9]+$");
        public string LineId => m_LineId;

        public VODialogLineSequence(StoryTeller storyTeller, string text, string section) : base(storyTeller, text, section) {
            Story story = storyTeller.Story;
            m_Text = text;

            if (story.currentTags.Count == 0) {
                Debug.LogWarning($"Untagged line found in {section}: {m_Text}");
                return;
            }

            if (!section.IsNullOrEmpty()) {
                section += ".";
            }

            string prefix = story.currentTags.FirstOrDefault(t => m_NumberRegex.IsMatch(t));

            // Force '1' -> '01', etc.
            if (!prefix.IsNullOrEmpty() && prefix.Length == 1 && Char.IsDigit(prefix[0])) {
                prefix = "0" + prefix;
            }

            m_LineId = section + prefix;
            m_Clip = Globals.VO.Get(m_LineId);
        }

        public VODialogLineSequence(StoryTeller storyTeller, string text, string section, string lineId)
            : base(storyTeller, text, section) {
            m_LineId = lineId;
            m_Clip = Globals.VO.Get(m_LineId);
        }

        public override void Start() {
            base.Start();

            if (m_Clip != null) {
                Play(m_Clip, m_Section);
            } else {
                Debug.LogWarning($"No lipsync VO file found for {m_LineId}: {m_Text}");
            }
        }

        protected override float GetDuration() {
            return m_Clip != null ? m_Clip.length + GetPunctuationPause(m_Text) : base.GetDuration();
        }

        public override void Interrupt() {
            base.Interrupt();
            Stop(m_Clip, m_Section);
        }

        protected override void OnComplete() {
            if (m_Clip == null) {
                base.OnComplete();
                return;
            }

            Log("Audio clip '" + m_Clip.name + "' duration complete.", m_Clip);
            IsComplete = true;
        }

        private void Play(LipSyncData clip, string section) {
            Log($"PLAYING VO: {section}.{clip.name}", clip);

            if (m_StoryTeller.FocusedCharacter is IPerformLipSync lipSyncCharacter) {
                m_Character = lipSyncCharacter;
                lipSyncCharacter.PlayLipSync(clip);
            } else {
                Debug.LogWarning("No audio source found for VO. Playing as a '2D' sound.");
                clip.clip.PlayOneShot();
            }
        }

        private void Stop(LipSyncData clip, string section) {
            Log($"STOPPING VO: {section}.{clip.name}", clip);
            
            if (m_Character != null) {
                m_Character.StopLipSync();
            } else {
                clip.clip.StopOneShot();
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
    }
}