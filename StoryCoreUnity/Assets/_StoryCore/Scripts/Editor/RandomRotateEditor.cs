using UnityEditor;
using UnityEngine;

namespace StoryCore.Utils {
    [CustomEditor(typeof(RandomRotate))]
    public class RandomRotateEditor : Editor<RandomRotate> {
        private void OnSceneGUI() {
            if (Target.AxisOverride.magnitude > 0 && Target.Renderer) {
                Vector3 start = Target.Renderer.bounds.center;
                Vector3 globalAxis = Target.transform.TransformDirection(Target.AxisOverride);
                Vector3 end = start + globalAxis.normalized*0.5f;
                // Handles.DrawLine(start, end);
                // Handles.color = Color.yellow;
                Handles.ArrowHandleCap(0, start, Quaternion.LookRotation(globalAxis), 0.5f, EventType.Repaint);
            }
        }
    }
}