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

        protected override bool UseSubtitles => !m_Clip || m_StoryTeller.UseSubtitles && base.UseSubtitles;
        private readonly Regex m_NumberRegex = new Regex(@"^[0-9]+$");

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
            if (!prefix.IsNullOrEmpty() && prefix.Length == 1 && char.IsDigit(prefix[0])) {
                prefix = "0" + prefix;
            }

            m_LineId = section + prefix;
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
            return m_Clip != null ? m_Clip.length + StoryTeller.GetPunctuationPause(m_Text) : base.GetDuration();
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
                lipSyncCharacter.PlayLipSync(clip);
            } else {
                Debug.LogWarning("No audio source found for VO. Playing as a '2D' sound.");
                clip.clip.PlayOneShot();
            }
        }
    }
}