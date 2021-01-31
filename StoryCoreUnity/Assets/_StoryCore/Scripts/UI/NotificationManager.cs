using System.Collections.Generic;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.UI {
    public class NotificationManager : Singleton<NotificationManager> {
        [SerializeField, AutoFillAsset] private NotificationUI m_NotificationUI;

        private NotificationUI m_CurrentInstance;

        protected virtual void CreateNotificationUI(string title, string text, float duration, List<string> tags) {
            m_CurrentInstance = Instantiate(m_NotificationUI);
            m_CurrentInstance.Show(title, text, duration, tags);
        }

        public static void Show(string title, string text, float duration = -1, List<string> tags = null) {
            Clear();
            Instance.CreateNotificationUI(title, text, duration, tags);
        }

        public static void Clear() {
            UnityUtils.DestroyObject(Instance.m_CurrentInstance);
        }
    }
}