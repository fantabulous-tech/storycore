using System;
using StoryCore.Choices;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.HeadGesture {
    [CreateAssetMenu(menuName = "Choices/Head Gesture Choice", fileName = "HeadGestureChoiceHandler", order = 0)]
    public class HeadGestureChoiceHandler : ChoiceHandler {
        private enum NodDirection {
            UpDown,
            LeftRight
        }

        [Tooltip("Head gesture direction that this handler handles.")]
        [SerializeField] private NodDirection m_NodDirection;

        [SerializeField] private GameVariableBool m_ShowFeedback;
        [SerializeField] private DirectionEdge m_DirectionEdgePrefab;
        [SerializeField] private RectTransform m_OnSuccessUIPrefab;

        [Tooltip("Where to put the feedback UI.")]
        [SerializeField] private Vector3 m_Offset = Vector3.forward;

        [Tooltip("The number of 'edge hits' before the nod counts as complete.")]
        [SerializeField, Range(1, 3)] private int m_RequiredNodCount = 2;

        [Tooltip("The window of time the player has to hit the next edge before the nod action times out.")]
        [SerializeField] private float m_MaxDuration = 1;

        [Tooltip("The number of degrees the player must move their head.")]
        [SerializeField] private float m_GestureAngle = 10;

        [Tooltip("The number of degrees in the wrong direction that will cancel the player's nod.")]
        [SerializeField] private float m_OffAngleMax = 10;

        private DirectionEdge[] m_Edges;
        private DirectionEdge m_LastEdgePulled;
        private int m_NodCount;
        private GameObject m_EdgeContainer;
        private DelaySequence m_TimeoutDelay;

        public Vector3 Offset => m_Offset;
        public float MaxDuration => m_MaxDuration;
        public float OffAngleMax => m_OffAngleMax;
        public float GestureAngle => m_GestureAngle;

        private GameObject EdgeContainer => UnityUtils.GetOrSet(ref m_EdgeContainer, () => new GameObject(m_NodDirection + " Head Gesture"));

        protected override void Init() {
            base.Init();
            Transform t = EdgeContainer.transform;
            
            switch (m_NodDirection) {
                case NodDirection.UpDown:
                    DirectionEdge up = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Up);
                    DirectionEdge down = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Down);
                    Setup(up, down);
                    break;
                case NodDirection.LeftRight:
                    DirectionEdge left = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Left);
                    DirectionEdge right = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Right);
                    Setup(left, right);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EdgeContainer.SetActive(false);
            DontDestroyOnLoad(EdgeContainer);
        }

        private void Setup(params DirectionEdge[] edges) {
            m_Edges = edges;

            m_Edges.ForEach(e => {
                e.LimitReached += OnLimitReached;
                e.OffCentered += OnOffCentered;
            });
        }

        public override void Ready(StoryChoice choice) {
            TryInit();
            EdgeContainer.SetActive(true);
        }

        protected override void PauseInternal() {
            base.PauseInternal();
            EdgeContainer.SetActive(false);
        }

        protected override void ResumeInternal() {
            base.ResumeInternal();
            EdgeContainer.SetActive(true);
        }

        public override void Cancel() {
            base.Cancel();
            ResetNod();
            EdgeContainer.SetActive(false);
        }

        private void Timeout() {
            m_NodCount = 0;
            m_LastEdgePulled = null;
            RaiseEvaluationFailed();
        }

        private void OnOffCentered(DirectionEdge edge) {
            ResetNod();
        }

        private void OnLimitReached(DirectionEdge edge) {
            if (m_LastEdgePulled != edge) {
                if (m_LastEdgePulled == null) {
                    // If this is the first time we are hitting an edge, then skip counting.
                    //Debug.Log($"{m_Name} nod started.");
                    RaiseEvaluating();
                } else {
                    m_NodCount++;
                    //Debug.Log($"{m_Name} Nods = {m_NodCount}");
                }

                if (m_NodCount > m_RequiredNodCount) {
                    Transform edgeTransform = m_LastEdgePulled ? m_LastEdgePulled.transform : edge.transform;
                    Instantiate(m_OnSuccessUIPrefab, edgeTransform.position, edgeTransform.rotation);
                    ResetNod();
                    Choose();
                } else {
                    m_LastEdgePulled = edge;
                    UpdateSprites();
                    RestartNodTimer(edge);
                }
            } else if (m_LastEdgePulled && m_LastEdgePulled.AtLimit) {
                RestartNodTimer(edge);
            }
        }

        private void RestartNodTimer(DirectionEdge edge) {
            m_TimeoutDelay?.Cancel("Starting a new nod.", this);

            // Only show the edge on the last nod.
            if (m_NodCount >= m_RequiredNodCount - 1 && m_ShowFeedback.Value) {
                edge.ResetFade();
            }

            m_TimeoutDelay = Delay.For(m_MaxDuration, this).InRealTime().Then(Timeout);
        }

        private void ResetNod() {
            //Debug.Log($"Resetting {m_Name}");
            m_TimeoutDelay?.Cancel("Cancelling previous nod timer.", this);
            m_NodCount = 0;
            m_Edges?.ForEach(e => e.Recenter());
            m_LastEdgePulled = null;
            UpdateSprites();
        }

        private void UpdateSprites() {
            m_Edges?.ForEach(e => e.SetSprite(m_LastEdgePulled == e));
        }
    }
}