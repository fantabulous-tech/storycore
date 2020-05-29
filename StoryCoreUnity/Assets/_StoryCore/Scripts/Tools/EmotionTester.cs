using System.Linq;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using StoryCore.Utils;
using TMPro;
using UnityEngine;

namespace StoryCore {
    public class EmotionTester : MonoBehaviour {
        [SerializeField, AutoFillAsset] private EmotionBucket m_EmotionBucket;
        [SerializeField] private TMP_Dropdown m_Dropdown;

        private int m_LastIndex = -1;
        private string[] EmotionList => m_EmotionBucket.ItemNames;
        private Character[] m_Characters;
        private float m_LastAmount = -1;

        public int Index { get; set; }

        public string Emotion => EmotionList[Index];

        public float Amount { get; private set; } = 1;

        private void Start() {
            m_Characters = FindObjectsOfType<Character>();
            Index = EmotionList.IndexOf("Neutral");
            m_LastIndex = Index;
            m_Dropdown.options = EmotionList
                                 .Select(g => new TMP_Dropdown.OptionData(g.ToLower()))
                                 .ToList();
        }

        private void Update() {
            CheckIndex();
        }

        public void SetAmount(float amount) {
            Amount = amount;
        }

        private void CheckIndex() {
            if (Index == m_LastIndex && Amount.Approximately(m_LastAmount)) {
                return;
            }

            m_LastIndex = Index;
            m_LastAmount = Amount;

            if (EmotionList == null || EmotionList.Length == 0) {
                Debug.LogWarning("No valid animation found.");
                return;
            }

            m_LastIndex = Index = UnityUtils.Mod(Index, EmotionList.Length);
            m_Dropdown.value = m_LastIndex;

            if (Emotion.IsNullOrEmpty()) {
                Debug.LogWarning("Emotion number " + Index + " is missing.");
                return;
            }

            if (m_Characters == null || m_Characters.Length == 0) {
                Debug.LogWarning("No characters found. Can't play " + Emotion);
                return;
            }

            foreach (Character character in m_Characters) {
                if (character) {
                    character.JumpToEmotion(Emotion, Emotion == "Neutral" ? 1 : Amount);
                }
            }
        }

        public void PreviousEmotion() {
            Index--;
        }

        public void NextEmotion() {
            Index++;
        }
    }
}