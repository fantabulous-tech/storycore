using StoryCore.Commands;
using UnityEngine;

namespace StoryCore.Locations {
    public abstract class BaseLocation : MonoBehaviour {
        [SerializeField] protected Vector3 m_PositionOffset;
        [SerializeField] protected Vector3 m_RotationOffset;

        protected abstract Color GizmoColor { get; }

        public virtual Vector3 Position => transform.TransformPoint(m_PositionOffset);
        public virtual Quaternion Rotation => transform.rotation*Quaternion.Euler(m_RotationOffset);

        public void OnEnable() {
            Buckets.locations.Add(this);
        }

        public void OnDisable() {
            if (Buckets.Exists) {
                Buckets.locations.Remove(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!enabled) {
                return;
            }
            Gizmos.color = GizmoColor;
            Gizmos.DrawSphere(Position, 0.1f);
            UnityEditor.Handles.color = GizmoColor;
            UnityEditor.Handles.ArrowHandleCap(0, Position, Rotation, 0.2f, EventType.Repaint);
        }
#endif
    }
}