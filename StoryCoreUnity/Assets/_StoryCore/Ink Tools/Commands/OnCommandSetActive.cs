using System;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    public class OnCommandSetActive : MonoBehaviour {
        [SerializeField] private string m_ObjectName;
        [SerializeField] private CommandHandler m_ShowCommand;
        [SerializeField] private CommandHandler m_HideCommand;

        private void Awake() {
            m_ShowCommand.Event += OnShow;
            m_HideCommand.Event += OnHide;
        }

        private void OnDestroy() {
            m_ShowCommand.Event -= OnShow;
            m_HideCommand.Event -= OnHide;
        }

        private void OnShow(string objName) {
            if (m_ObjectName.IsNullOrEmpty() || m_ObjectName.Equals(objName, StringComparison.OrdinalIgnoreCase)) {
                gameObject.SetActive(true);
            }
        }

        private void OnHide(string objName) {
            if (m_ObjectName.IsNullOrEmpty() || m_ObjectName.Equals(objName, StringComparison.OrdinalIgnoreCase)) {
                gameObject.SetActive(false);
            }
        }
    }
}