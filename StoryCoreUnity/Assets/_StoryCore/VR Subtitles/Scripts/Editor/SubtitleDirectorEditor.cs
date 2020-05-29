using UnityEditor;

namespace VRSubtitles {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in SubtitleDirector to be expandable in the Inspector.
    [CustomEditor(typeof(SubtitleDirector))]
    public class SubtitleDirectorEditor : Editor { }
}