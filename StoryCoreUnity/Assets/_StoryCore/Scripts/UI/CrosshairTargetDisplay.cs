using TMPro;
using UnityEngine;

namespace StoryCore.UI {
    public class CrosshairTargetDisplay : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_Text;

        private void Update() {
            if (m_Text != null) {
                m_Text.text = CrosshairTarget.Target ? $"{CrosshairTarget.Target.name}\nDistance: {CrosshairTarget.Distance:N2}" : "";
            }
        }
    }
}