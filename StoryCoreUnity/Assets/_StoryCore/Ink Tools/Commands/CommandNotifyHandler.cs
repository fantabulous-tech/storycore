using CoreUtils;
using StoryCore.UI;
using StoryCore.Utils;
using UnityEngine;
using VRSubtitles;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Notify")]
    public class CommandNotifyHandler : CommandHandler {
        [SerializeField] private NotificationUI m_NotificationPrefab;

        public override DelaySequence Run(ScriptCommandInfo info) {
            SubtitleDirector.Clear();
            
            string title = info.NamedParams.ContainsKey("title") ? info.NamedParams["title"] : "";

            string text;
            if (info.NamedParams.ContainsKey("text")) {
                text = info.NamedParams["text"];
            } else {
                text = info.NamedParams.Count == 0 ? info.Params.AggregateToString(" ") : "";
            }

            float delay = info.NamedParams.ContainsKey("delay") && float.TryParse(info.NamedParams["delay"], out float d) ? d : -1;

            NotificationManager.Show(title, text, delay, info.StoryTags);
            return DelaySequence.Empty;
        }
    }
}