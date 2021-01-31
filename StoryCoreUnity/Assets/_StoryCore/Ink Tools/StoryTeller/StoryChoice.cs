using System;
using System.Linq;
using CoreUtils;
using StoryCore.Utils;
using Ink.Runtime;
using Polyglot;
using UnityEngine;

namespace StoryCore {
    [Serializable]
    public class StoryChoice : ILocalize {
        private readonly Choice m_InkChoice;
        private readonly StoryTeller m_StoryTeller;

        private string m_ChoiceName;
        private string m_DisplayText;
        private string m_Key;
        private string[] m_Params;

        public int Index => m_InkChoice.index;

        public string Key => UnityUtils.GetOrSet(ref m_Key, () => m_InkChoice.text.Replace(" ", "").TrimEnd('!'));

        // Remove text in parenthesis and additional parameters
        public string DisplayText => UnityUtils.GetOrSet(ref m_DisplayText, GetLocalizedText);

        public string ChoiceName => UnityUtils.GetOrSet(ref m_ChoiceName, () => GetChoicePieces().First().TrimEnd('!'));

        public string[] ChoiceParams => UnityUtils.GetOrSet(ref m_Params, () => GetChoicePieces().Skip(1).ToArray());

        public bool CanInterrupt => m_InkChoice.text.EndsWith("!");

        public bool IsValidChoice(string choiceName) {
            if (choiceName.IsNullOrEmpty()) {
                return false;
            }

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
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        ~StoryChoice() {
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        private string[] GetChoicePieces() {
            return m_InkChoice.text.Split(new[] {' ', '\t', ':'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public void Choose() {
            m_StoryTeller.Choose(this);
        }

        public string GetLocalizedText() {
            string choiceID = "CHOICE_" + ChoiceName.ToUpper();
            string localization = Localization.Get(choiceID);

            if (Localization.KeyExist(choiceID) && !localization.IsNullOrEmpty()) {
                
                // Special handling for localization of 'Pose' options.
                if (ChoiceName.Equals("pose", StringComparison.OrdinalIgnoreCase) && ChoiceParams.Any()) {
                    string poseKey = $"POSE_{ChoiceParams[0].ToUpper()}";
                    string localizedPose = Localization.Get(poseKey);
                    return Localization.KeyExist(poseKey) && !localizedPose.IsNullOrEmpty() ? $"{localization}: {localizedPose}" : $"{localization}: {ChoiceParams[0].ToSpacedName()}";
                }
                
                // Special handling for localization of 'Move' options.
                if (ChoiceName.Equals("move", StringComparison.OrdinalIgnoreCase) && ChoiceParams.Any()) {
                    string moveKey = $"MOVE_{ChoiceParams[0].ToUpper()}";
                    string localizedMove = Localization.Get(moveKey);
                    if (Localization.KeyExist(moveKey) && !localizedMove.IsNullOrEmpty()) {
                        return $"{localization}: {localizedMove}";
                    } else {
                        Debug.LogWarning($"Couldn't find translation ID for {m_InkChoice.text} (expected '{moveKey}')");
                        return $"{localization}: {ChoiceParams[0].ToSpacedName()}";
                    }
                }
                
                return localization;
            }

            Debug.LogWarning($"Couldn't find translation ID for {m_InkChoice.text} (expected '{choiceID}')");
            return m_InkChoice.text.ToSpacedName(true, false).Replace(":", ": ");
        }

        public void OnLocalize() {
            m_DisplayText = GetLocalizedText();
        }

        public override string ToString() {
            return $"{base.ToString()}: {Key}";
        }
    }
}