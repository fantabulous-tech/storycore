using System;
using CoreUtils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StoryCore.HeadGesture {
    public class DirectionEdge : MonoBehaviour {
        [SerializeField] private GameObject m_UpUI;
        [SerializeField] private GameObject m_DownUI;
        [SerializeField] private GameObject m_LeftUI;
        [SerializeField] private GameObject m_RightUI;
        [SerializeField] private CanvasGroup m_Fader;
        [SerializeField, ReadOnly] private bool m_AtLimit;

        private HeadGestureTracker m_Tracker;
        private Vector3 m_CenterOffDirection;
        private GameObject m_UI;
        private Vector3 m_Axis;
        private Vector3 m_OffAxis;
        private bool m_Init;
        private float m_FadeStart;
        private float m_Angle;
        private float m_OffAngle;


        public event Action<DirectionEdge> LimitReached;
        public event Action<DirectionEdge> OffCentered;

        public bool AtLimit => m_AtLimit;

        private Vector3 GlobalAxis => Head ? Head.TransformDirection(m_Axis) : m_Axis;
        private Vector3 GlobalOffAxis => Head ? Head.TransformDirection(m_OffAxis) : m_OffAxis;
        private Transform Head => HeadGestureTracker.Head;
        private Vector3 Offset => m_Tracker.Offset;

        // TODO: Optimize Angle/OffAngle into a head-space euler angle Vector2 calculated once per frame.

        private float Angle => m_Angle;

        private float OffAngle => m_OffAngle;

        public DirectionEdge Init(HeadGestureTracker tracker, Direction direction) {
            name = direction + " Edge";

            m_Init = true;
            m_Tracker = tracker;

            m_UpUI.SetActive(false);
            m_DownUI.SetActive(false);
            m_LeftUI.SetActive(false);
            m_RightUI.SetActive(false);

            switch (direction) {
                case Direction.Up:
                    m_OffAxis = Vector3.up;
                    m_Axis = Vector3.right;
                    m_UI = m_UpUI;
                    break;
                case Direction.Down:
                    m_OffAxis = Vector3.down;
                    m_Axis = Vector3.left;
                    m_UI = m_DownUI;
                    break;
                case Direction.Left:
                    m_OffAxis = Vector3.left;
                    m_Axis = Vector3.up;
                    m_UI = m_LeftUI;
                    break;
                case Direction.Right:
                    m_OffAxis = Vector3.right;
                    m_Axis = Vector3.down;
                    m_UI = m_RightUI;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return this;
        }

        private void LateUpdate() {
            if (!m_Init) {
                return;
            }

            UpdateAngles();
            m_Fader.alpha = 1 - Mathf.Clamp01((Time.unscaledTime - m_FadeStart)/m_Tracker.MaxDuration);

            // Check to see if angle is too far off.
            if (OffAngle > m_Tracker.OffAngleMax) {
                OffCenter();
            }

            // Check if we need to push the edge to follow the head.
            else if (Angle < 0) {
                Recenter();
            }

            // Check if we need to drag the edge along with the head.
            else if (Angle >= m_Tracker.GestureAngle) {
                PullPos();
            } else if (Angle < m_Tracker.GestureAngle/2 && m_AtLimit) {
                m_AtLimit = false;
                //Debug.LogFormat(this, "{0} NOT at limit anymore.", name);
            }
            
            UpdateAngles();

            // Finally, update to the new position.
            UpdatePosition();
        }

        private void UpdateAngles() {
            Vector3 axis = GlobalAxis;
            Vector3 direction = Vector3.ProjectOnPlane(transform.position - Head.position, axis);
            m_Angle = -Vector3.SignedAngle(Head.forward, direction, axis);
            m_OffAngle = Vector3.Angle(axis, m_CenterOffDirection);
        }

        private void OffCenter() {
            Recenter();
            OffCentered?.Invoke(this);
        }

        private void UpdatePosition() {
            transform.rotation = Head.rotation;
            Vector3 rotatedOffset = Quaternion.AngleAxis(-Angle, m_Axis)*Offset;
            transform.position = Head.position + Head.TransformDirection(rotatedOffset);
        }

        private void PullPos() {
            Vector3 rotatedOffset = Quaternion.AngleAxis(-m_Tracker.GestureAngle, m_Axis)*Offset;
            Vector3 pos = Head.position + Head.TransformDirection(rotatedOffset);
            transform.position = pos;

            if (!m_AtLimit) {
                //Debug.LogFormat(this, "{0} limit reached.", name);
                LimitReached?.Invoke(this);
                m_AtLimit = true;
            }
        }

        public void Recenter() {
            m_AtLimit = false;
            m_CenterOffDirection = GlobalAxis;
            transform.position = Head.position + Head.TransformDirection(Offset);
            m_UI.SetActive(false);
            UpdateAngles();
        }

        public void SetSprite(bool showSprite) {
            m_UI.SetActive(showSprite);
        }

#if UNITY_EDITOR
        public void OnDrawEdgeGizmo() {
            if (!Application.isPlaying || !gameObject.activeInHierarchy) {
                return;
            }

            Vector3 position = transform.position + GlobalOffAxis*0.25f;
            Handles.Label(position, $"{name}\nDegrees: {Angle:N0}\nOff Degrees: {OffAngle:N0}");
            Handles.DoPositionHandle(position, Head.rotation);

            // Handles.color = Color.green;
            // Handles.DrawSolidDisc(position, GlobalAxis, 0.05f);
            // Handles.color = Color.red;
            // Handles.DrawSolidDisc(position, GlobalOffAxis, 0.05f);
        }
#endif

        public void ResetFade() {
            m_FadeStart = Time.unscaledTime;
        }
    }

    public enum Direction {
        Up,
        Down,
        Left,
        Right
    }
}