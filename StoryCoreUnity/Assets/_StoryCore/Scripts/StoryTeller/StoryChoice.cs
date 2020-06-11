using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.Utils;
using Ink.Runtime;

namespace StoryCore {
    [Serializable]
    public class StoryChoice {
        private readonly Choice m_InkChoice;
        private readonly StoryTeller m_StoryTeller;

        public int Index => m_InkChoice.index;

        public string Text => m_InkChoice.text.TrimEnd('!');

        // Remove text in parenthesis and additional parameters
        public string DisplayText => Text.ReplaceRegex(@"(\([^\)].*\)|\b [^:].*)", "");

        private string ChoiceName => GetChoicePieces().First().TrimEnd('!');
        
        public IEnumerable<string> ChoiceParams => GetChoicePieces().Skip(1);

        public bool IsValidChoice(string choiceName) {
            choiceName = choiceName.Replace(" ", "");

            if (string.Equals(ChoiceName, choiceName, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            // Try to match the command + first parameter. (e.g. 'pose:stand')
            return string.Equals(ChoiceName + ":" + ChoiceParams.FirstOrDefault(), choiceName);
        }

        public StoryChoice(Choice inkChoice, StoryTeller storyTeller) {
            m_InkChoice = inkChoice;
            m_StoryTeller = storyTeller;
        }

        private IEnumerable<string> GetChoicePieces() {
            return m_InkChoice.text.Split(new[] {' ', '\t', ':'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public void Select() {
            m_StoryTeller.Choose(this);
        }
    }
}