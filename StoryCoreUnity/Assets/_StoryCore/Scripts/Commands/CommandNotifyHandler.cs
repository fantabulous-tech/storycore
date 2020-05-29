using StoryCore.Utils;
using UnityEngine;
using VRSubtitles;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Notify")]
    public class CommandNotifyHandler : CommandHandler {
        [SerializeField] private NotificationUI m_NotificationPrefab;

        private NotificationUI m_Instance;

        private void RemoveNotice() {
            UnityUtils.DestroyObject(m_Instance);
        }

        public override DelaySequence Run(ScriptCommandInfo info) {
            RemoveNotice();
            SubtitleDirector.Clear();
            m_Instance = Instantiate(m_NotificationPrefab);
            m_Instance.Show(info);
            return DelaySequence.Empty;
        }
    }
}