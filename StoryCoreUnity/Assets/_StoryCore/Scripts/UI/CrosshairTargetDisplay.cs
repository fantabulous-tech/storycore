using TMPro;
using UnityEngine;

namespace StoryCore.UI {
    public class CrosshairTargetDisplay : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_Text;
		[SerializeField] private CrosshairTarget m_CrosshairTarget;
		
        private void Update() {
            if (m_Text != null) {
                m_Text.text = m_CrosshairTarget.Target ? $"{m_CrosshairTarget.Target.name}\nDistance: {m_CrosshairTarget.Distance:N2}" : "";
            }
        }
    }
}