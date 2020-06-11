using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.AssetUsage {
	public static class CustomEditorStyles {
		private static GUIStyle s_ButtonLeft;

		public static GUIStyle ButtonLeft => UnityUtils.GetOrSet(ref s_ButtonLeft, () => new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleLeft});
	}
}