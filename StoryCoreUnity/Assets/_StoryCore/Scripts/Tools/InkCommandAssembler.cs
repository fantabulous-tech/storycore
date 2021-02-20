using UnityEngine;
using UnityEngine.Serialization;

namespace StoryCore {
    public class InkCommandAssembler : MonoBehaviour {
        [SerializeField] private EmotionTester m_EmotionTester;
        [FormerlySerializedAs("m_PerformanceTester"),SerializeField] private CharacterTester m_CharacterTester;

        public void CopyToClipboard() {
            string performance = m_CharacterTester.PerformanceName.ToLower();
            string emotion = m_EmotionTester.Emotion.ToLower();
            string amount = m_EmotionTester.Amount < 100 ? (m_EmotionTester.Amount*100).ToString("N0") : "";

            if (m_EmotionTester.Amount <= 0 || emotion == "neutral") {
                GUIUtility.systemCopyBuffer = $"/perform {performance} neutral";
            } else {
                GUIUtility.systemCopyBuffer = $"/perform {performance} {emotion} {amount}".Trim();
            }
        }
    }
}