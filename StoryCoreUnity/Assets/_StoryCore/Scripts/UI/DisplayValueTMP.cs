using CoreUtils;
using StoryCore.Utils;
using TMPro;
using UnityEngine;

public class DisplayValueTMP : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private float m_Scaler = 1;
    [SerializeField] private string m_Format = "N0";

    public void SetValue(float value) {
        value *= m_Scaler;

        if (m_Text) {
            m_Text.text = !m_Format.IsNullOrEmpty() ? value.ToString(m_Format) : value.ToString();
        }
    }
}