using System.Linq;
using StoryCore.Utils;
#if STEAM_BUILD
using Steamworks;
#endif
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Open Link")]
    public class CommandOpenLinkHandler : CommandHandler {
        public override DelaySequence Run(ScriptCommandInfo info) {
            string url = info.Params.ElementAtOrDefault(0);
            if (!url.ContainsRegex(@"^https?://")) {
                url = "http://" + url;
            }

#if STEAM_BUILD
			SteamFriends.ActivateGameOverlayToWebPage(url);
#else
            Application.OpenURL(url);
#endif

            Raise(url);

            return DelaySequence.Empty;
        }
    }
}