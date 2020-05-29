using UnityEditor;

namespace Bindings {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in ToggleBinding to be expandable in the Inspector.
    [CustomEditor(typeof(ToggleBinding))]
    public class ToggleBindingEditor : Editor { }
}