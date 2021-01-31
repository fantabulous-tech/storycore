using UnityEditor;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CustomEditor(typeof(VOBucket))]
    public class VOBucketEditor : AssetBucketEditor {
        protected override void OnAssetGUI(AssetListItem item) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.Name, GUILayout.Width(m_ObjectColumnWidth));
            GUILayout.Label(item.DisplayPath);
            GUILayout.EndHorizontal();
        }
    }
}