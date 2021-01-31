using CoreUtils;
using JetBrains.Annotations;
using StoryCore.Utils;
using TMPro;
using UnityEngine;

public class VersionLabel : MonoBehaviour {
    [SerializeField, AutoFill] private TextMeshProUGUI m_Label;

    private void OnEnable() {
        if (m_Label) {
            m_Label.text = $"{Application.productName}\nv.{Application.version}";
        }
    }

    [UsedImplicitly]
    public void CopyVersionToClipboard() {
        GUIUtility.systemCopyBuffer = $"{Application.productName} v.{Application.version}";
    }
}