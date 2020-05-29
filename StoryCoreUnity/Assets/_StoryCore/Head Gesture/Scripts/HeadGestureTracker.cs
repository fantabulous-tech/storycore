using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace StoryCore.HeadGesture {
    public class HeadGestureTracker : MonoBehaviour {
        [SerializeField] private GameVariableBool m_ShowFeedback;
        [SerializeField] private DirectionEdge m_DirectionEdgePrefab;

        [Tooltip("Where to put the feedback UI.")]
        [SerializeField] private Vector3 m_Offset = Vector3.forward;

        [Tooltip("The number of 'edge hits' before the nod counts as complete.")]
        [SerializeField, Range(1, 3)] private int m_RequiredNodCount = 2;

        [Tooltip("The window of time the player has to hit the next edge before the nod action times out.")]
        [SerializeField] private float m_MaxDuration = 1;

        [Tooltip("The number of degrees the player must move their head.")]
        [SerializeField] private float m_GestureAngle = 5;

        [Tooltip("The number of degrees in the wrong direction that will cancel the player's nod.")]
        [SerializeField] private float m_OffAngleMax = 3;

        [Tooltip("The number of seconds before the next head gesture will be tracked.")]
        [SerializeField] private float m_CoolDown = 1;

        [Tooltip("The canvas group that holds the yes/no answer UI game objects.")]
        [SerializeField] private CanvasGroup m_UI;

        [Tooltip("The child 'Yes' UI game object.")]
        [SerializeField] private GameObject m_YesAnswerUI;

        [Tooltip("The child 'No' UI game object.")]
        [SerializeField] private GameObject m_NoAnswerUI;

        public UnityEvent OnYes;
        public UnityEvent OnNo;

        private DirectionEdge[] m_Edges;
        private NodWatcher m_YesWatcher;
        private NodWatcher m_NoWatcher;
        private float m_LastSuccessTime;
        private Transform m_Head;
        private bool m_Init;

        public int RequiredNodCount => m_RequiredNodCount;
        public Vector3 Offset => m_Offset;
        public float OffAngleMax => m_OffAngleMax;
        public float GestureAngle => m_GestureAngle;
        public float MaxDuration => m_MaxDuration;
        public bool ShowFeedback => m_ShowFeedback.Value;

        public static Transform Head => UnityUtils.CameraTransform;

        private bool Init() {
            if (!Head) {
                return false;
            }

            if (m_Init) {
                return true;
            }

            m_Init = true;
            Transform t = transform;
            t.position = Head.position;
            t.rotation = Head.rotation;

            m_UI.gameObject.SetActive(false);

            DirectionEdge up = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Up);
            DirectionEdge down = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Down);
            DirectionEdge right = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Right);
            DirectionEdge left = Instantiate(m_DirectionEdgePrefab, t).Init(this, Direction.Left);

            m_Edges = new[] {up, down, right, left};

            m_YesWatcher = new NodWatcher("Yes", new[] {up, down}, this);
            m_YesWatcher.Nodded += OnYesGesture;

            m_NoWatcher = new NodWatcher("No", new[] {left, right}, this);
            m_NoWatcher.Nodded += OnNoGesture;

            ResetNod();
            return true;
        }

        private void Start() {
            m_UI.gameObject.SetActive(false);
        }

        private void OnEnable() {
            ResetNod();
            m_Edges.ForEach(e => e.gameObject.SetActive(true));
        }

        private void OnDisable() {
            m_Edges.ForEach(e => e.gameObject.SetActive(false));
            ResetNod();
        }

        private void LateUpdate() {
            if (!Init()) {
                return;
            }

            // Delay listening to for the next gesture.
            if (m_LastSuccessTime + m_CoolDown > Time.unscaledTime) {
                m_Edges.ForEach(e => e.Recenter());
                m_UI.gameObject.SetActive(ShowFeedback);
                m_UI.alpha = 1 - Mathf.Clamp01((Time.unscaledTime - m_LastSuccessTime)/m_CoolDown);
                return;
            }

            transform.position = Head.position;
            m_UI.gameObject.SetActive(false);
            m_YesWatcher.Update();
            m_NoWatcher.Update();
        }

        private void OnYesGesture(DirectionEdge edge) {
            Debug.Log("Yes!");
            OnYes.Invoke();
            m_LastSuccessTime = Time.unscaledTime;
            Transform edgeTransform = edge.transform;
            m_UI.transform.SetPositionAndRotation(edgeTransform.position, edgeTransform.rotation);
            m_UI.gameObject.SetActive(ShowFeedback);
            m_YesAnswerUI.SetActive(true);
            m_NoAnswerUI.SetActive(false);
        }

        private void OnNoGesture(DirectionEdge edge) {
            Debug.Log("No!");
            OnNo.Invoke();
            m_LastSuccessTime = Time.unscaledTime;
            Transform edgeTransform = edge.transform;
            m_UI.transform.SetPositionAndRotation(edgeTransform.position, edgeTransform.rotation);
            m_UI.gameObject.SetActive(ShowFeedback);
            m_NoAnswerUI.SetActive(true);
            m_YesAnswerUI.SetActive(false);
        }

        public void ResetNod() {
            if (!Init()) {
                return;
            }

            m_YesWatcher.ResetNod();
            m_NoWatcher.ResetNod();
            m_UI.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            m_Edges.ForEach(e => e.OnDrawEdgeGizmo());
        }
#endif
    }
}