using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore.Commands {

    /*
     * Controls NPC character movement by adjusting AnimationController
     * parameters and adjust blending into the custom performances 
     */
    public class Locomotion : MonoBehaviour {

        [SerializeField] private Animator m_Animator;
        [SerializeField] private GameObject m_CharacterGameObject;
        [SerializeField] private float m_MinimumDistance = 0.2f;
        [SerializeField] private float m_MinimumSpeed = 1.0f;
        [SerializeField] private float m_MinimumAngleDifference = 20.0f;
        [SerializeField] private float m_MinimumStoppingTime = 1.0f;

        [SerializeField] private float m_Acceleration = 4.0f;
        [SerializeField] private float m_Decceleration = 0.5f;
        //[SerializeField] private GameObject m_DebugMoveTo;

        private int kVelocityXParam = Animator.StringToHash("VelocityX");
        private int kVelocityZParam = Animator.StringToHash("VelocityZ");
        private int kTurnParam = Animator.StringToHash("Turn");

        private Vector3 m_MoveTarget;
        private GameVariableVector3 m_MoveTargetVariable;
        private float m_TargetSpeed;
        private bool m_TurnToTarget;
        private float m_CurrentSpeed;

        public bool IsMoving { get; private set; }

        private void Start() {
            IsMoving = false;
            m_CurrentSpeed = 0.0f;
            m_MoveTargetVariable = null;
            m_Animator.SetFloat(kVelocityXParam, 0.0f);
            m_Animator.SetFloat(kVelocityZParam, 0.0f);
            m_Animator.SetFloat(kTurnParam, 0.0f);
        }

        public void MoveTo(Vector3 destinationToMoveTo, float speed, bool turn) {
            m_MoveTarget = destinationToMoveTo;
            m_MoveTargetVariable = null;
            m_TargetSpeed = speed;
            m_TurnToTarget = turn;
            IsMoving = true;
        }

        public void MoveTo(GameVariableVector3 destinationToMoveTo, float speed, bool turn) {
            m_MoveTargetVariable = destinationToMoveTo;
            m_TargetSpeed = speed;
            m_TurnToTarget = turn;
            IsMoving = true;
        }

        private void Update() {
            if (IsMoving) {
                if (m_MoveTargetVariable != null) {
                    m_MoveTarget = m_MoveTargetVariable.Value;
                }

                // remove any y offset, no movement up and down stairs
                m_MoveTarget.y = m_CharacterGameObject.transform.position.y;

                // check if we have made it to our destination
                float remainingDistance = (m_CharacterGameObject.transform.position - m_MoveTarget).magnitude;

                if (remainingDistance < m_MinimumDistance) {
                    IsMoving = false;
                    m_Animator.SetFloat(kVelocityZParam, 0.0f);
                    m_Animator.SetFloat(kVelocityXParam, 0.0f);
                } else {
                    // blend down speed when we are close to target

                    float timeTillTarget = remainingDistance/m_CurrentSpeed;
                    if (timeTillTarget < m_MinimumStoppingTime) {
                        m_CurrentSpeed -= m_Decceleration*Time.deltaTime;
                        if (m_CurrentSpeed < m_MinimumSpeed) {
                            m_CurrentSpeed = m_MinimumSpeed;
                        }
                    } else {
                        float additionalSpeed = m_Acceleration*Time.deltaTime;
                        // otherwise increase speed to max desired
                        if (m_TargetSpeed - m_CurrentSpeed < additionalSpeed) {
                            m_CurrentSpeed = m_TargetSpeed;
                        } else {
                            m_CurrentSpeed += additionalSpeed;
                        }
                    }

                    // get worldspace movement vector
                    Vector3 movementDirection = (m_MoveTarget - m_CharacterGameObject.transform.position).normalized;
                    movementDirection *= m_CurrentSpeed;

                    // change vector to local space
                    movementDirection = m_CharacterGameObject.transform.InverseTransformVector(movementDirection);

                    // set animation parameters to try and move us to where we need to be
                    // actual object movement is controlled by the animation root motion
                    m_Animator.SetFloat(kVelocityZParam, movementDirection.z);
                    m_Animator.SetFloat(kVelocityXParam, movementDirection.x);

                    // check if we want to turn to the target also
                    float angleOffset = Vector3.Angle(movementDirection, Vector3.forward);
                    if (m_TurnToTarget) {
                        if (angleOffset > m_MinimumAngleDifference) {
                            m_Animator.SetFloat(kTurnParam, Mathf.Sign(movementDirection.x)*angleOffset/90.0f);
                        } else {
                            m_Animator.SetFloat(kTurnParam, 0.0f);
                        }
                    }
                }
            }
        }

        // private void OnGUI() {
        // 	if (GUI.Button(new Rect(10, 10, 150, 50), "Move")) {
        // 		MoveTo(m_DebugMoveTo.transform.position, 3.0f, true);
        // 	}
        // }

    }
}