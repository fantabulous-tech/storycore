using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Utils {
    public class OpenUrl : MonoBehaviour {
        [SerializeField] private string m_Url;

        [SerializeField, AutoFillAsset(DefaultName = "InVR")]
        private GameVariableBool m_InVR;

        public void Go() {
            Application.OpenURL(m_Url);

            if (m_InVR && m_InVR.Value) {
                CommandManager.RunCommand("/notify title=\"URL Opened\" text=\"The URL has been opened on your desktop. Take off the headset to see the website.\" delay=10");
            }
        }
    }
}