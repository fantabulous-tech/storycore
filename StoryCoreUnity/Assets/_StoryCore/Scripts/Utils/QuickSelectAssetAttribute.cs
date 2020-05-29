using UnityEngine;

namespace StoryCore.Utils {
    public class QuickSelectAssetAttribute : PropertyAttribute {
        public string Filter { get; private set; }

        public QuickSelectAssetAttribute(string filter = "") {
            Filter = filter;
        }
    }
}