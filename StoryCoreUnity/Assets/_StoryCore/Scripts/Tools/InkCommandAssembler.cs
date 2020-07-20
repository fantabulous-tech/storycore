using StoryCore.Characters;
using UnityEngine;

namespace StoryCore {
    public class InkCommandAssembler : MonoBehaviour {
        [SerializeField] private EmotionTester m_EmotionTester;
        [SerializeField] private PerformanceTester m_PerformanceTester;

        public void CopyToClipboard() {
            string performance = m_PerformanceTester.PerformanceName.ToLower();
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