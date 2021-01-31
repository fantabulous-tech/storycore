using System.Linq;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Open Link")]
    public class CommandOpenLinkHandler : CommandHandler {
        public override DelaySequence Run(ScriptCommandInfo info) {
            string url = info.Params.ElementAtOrDefault(0);
            if (!url.ContainsRegex(@"^https?://")) {
                url = "http://" + url;
            }

            Application.OpenURL(url);
            Raise(url);
            return DelaySequence.Empty;
        }
    }
}