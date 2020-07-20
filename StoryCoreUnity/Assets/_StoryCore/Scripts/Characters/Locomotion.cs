using System;
using StoryCore.Locations;
using UnityEngine;
using UnityEngine.AI;

namespace StoryCore.Characters {

    /*
     * Controls NPC character movement by adjusting AnimationController
     * parameters and adjust blending into the custom performances 
     */
    public class Locomotion : MonoBehaviour {

        [SerializeField] private Animator m_Animator;
        [SerializeField] private GameObject m_CharacterGameObject;
        [SerializeField] private BaseCharacter m_Character;
        [SerializeField] private float m_MinimumDistance = 0.2f;
        [SerializeField] private float m_MinimumSpeed = 1.0f;
        [SerializeField] private float m_MinimumAngleDifference = 20.0f;
        [SerializeField] private float m_MinimumStoppingTime = 1.0f;
        
        [SerializeField] private float m_Acceleration = 4.0f;
        [SerializeField] private float m_Decceleration = 0.5f;
        //[SerializeField] private GameObject m_DebugMoveTo;

        [SerializeField] private float m_TurnToTargetMinDistance = 0.5f;
        
        enum LocomotionState {
            NotMoving,
            TurnToWalkingDirection,
            SideStepping,
            Walking,
            TurnToTargetDirection,
        }

        private LocomotionState m_LocomotionState;
        
        private int kVelocityXParam = Animator.StringToHash("VelocityX");
        private int kVelocityZParam = Animator.StringToHash("VelocityZ");
        private int kTurnParam = Animator.StringToHash("Turn");

        private float m_VelocityXSmoothed = 0.0f;
        private float m_VelocityZSmoothed = 0.0f;
        private float m_TurnSmoothed = 0.0f;
        
        private BaseLocation m_MoveTargetLocation;
        private float m_TargetSpeed;
        private float m_CurrentSpeed;
        private float m_DesiredDistanceFromTarget;

        private NavMeshPath m_NavPath;
        private int m_CurrentNavSegment = -1;
        
        public bool IsMoving => m_LocomotionState != LocomotionState.NotMoving;

        private void Start()
        {
            m_LocomotionState = LocomotionState.NotMoving;
            m_CurrentSpeed = 0.0f;
            m_MoveTargetLocation = null;
            m_Animator.SetFloat(kVelocityXParam, 0.0f);
            m_Animator.SetFloat(kVelocityZParam, 0.0f);
            m_Animator.SetFloat(kTurnParam, 0.0f);

            if (m_Character == null) {
                m_Character = GetComponent<BaseCharacter>();
            }
            
            m_NavPath = new NavMeshPath();
        }


        public void MoveTo(BaseLocation locationToMoveTo, float speed, float minDistance) {
            m_MoveTargetLocation = locationToMoveTo;
            m_TargetSpeed = speed;
            m_DesiredDistanceFromTarget = minDistance;


            UpdateNavPath();
            
            DecideMovementType();
        }

        private void UpdateNavPath()
        {
            NavMesh.CalculatePath(transform.position, m_MoveTargetLocation.Position, NavMesh.AllAreas, m_NavPath);
            m_CurrentNavSegment = 1;
        }

        private Vector3 GetCurrentTargetPosition()
        {
            if (m_NavPath == null || m_NavPath.corners.Length < 2)
            {
                return Vector3.zero;
            }

            return m_NavPath.corners[m_CurrentNavSegment];
        }

        private void DecideMovementType() {
            // decide if this is a small distant to travel and we don't really need to 
            // turn in the direction of movement
            
            if ((GetCurrentTargetPosition() - m_CharacterGameObject.transform.position).magnitude < m_TurnToTargetMinDistance) {
                // do little steps, no need to turn and walk for half a metre
                m_LocomotionState = LocomotionState.SideStepping;
            } else {
                m_Character.PauseLookAt();
                m_LocomotionState = LocomotionState.TurnToWalkingDirection;
            }
        }

        private void FixedUpdate() {

            float velX = Mathf.Lerp(m_Animator.GetFloat(kVelocityXParam), m_VelocityXSmoothed, 0.9f * Time.fixedDeltaTime*10.0f);
            m_Animator.SetFloat(kVelocityXParam, velX);
            float velZ = Mathf.Lerp(m_Animator.GetFloat(kVelocityZParam), m_VelocityZSmoothed, 0.9f * Time.fixedDeltaTime*10.0f);
            m_Animator.SetFloat(kVelocityZParam, velZ);
            float turn = Mathf.Lerp(m_Animator.GetFloat(kTurnParam), m_TurnSmoothed, 0.9f * Time.fixedDeltaTime*10.0f);
            m_Animator.SetFloat(kTurnParam, turn);

            switch (m_LocomotionState) {
                case LocomotionState.NotMoving:
                    // Do nothing
                    break;
                case LocomotionState.TurnToWalkingDirection: {
                    UpdateNavPath();
                    
                    // check if we are facing close enough to our target direction
                    Vector3 targetDirection = (GetCurrentTargetPosition() - m_CharacterGameObject.transform.position).normalized;
                    // change vector to local space
                    targetDirection = m_CharacterGameObject.transform.InverseTransformVector(targetDirection);

                    // check if we want to turn to the target also
                    float angleOffset = Vector3.Angle(targetDirection, Vector3.forward);
                    if (angleOffset > m_MinimumAngleDifference) {
                        m_TurnSmoothed = Mathf.Sign(targetDirection.x)*angleOffset/45.0f;
                    } else {
                        m_TurnSmoothed = 0.0f;
                        m_LocomotionState = LocomotionState.Walking;
                    }
                    break;
                }
                case LocomotionState.Walking:
                case LocomotionState.SideStepping: {
                    UpdateNavPath();
                    Vector3 moveTargetPosition = GetCurrentTargetPosition();
                    Quaternion moveTargetRotation = m_MoveTargetLocation.Rotation;
                    
                    // remove any y offset, no movement up and down stairs
                    moveTargetPosition.y = m_CharacterGameObject.transform.position.y;

                    bool isAtTargetPosition = false;
                    bool isAtTargetRotation = false;
                    
                    Vector3 movementDirection = (moveTargetPosition - m_CharacterGameObject.transform.position).normalized;
                    // find a point on the target radius where we will actually move to
                    float totalDist = (moveTargetPosition - m_CharacterGameObject.transform.position).magnitude;
                    float clampedDist = totalDist < m_DesiredDistanceFromTarget ? 0 : totalDist - m_DesiredDistanceFromTarget;
                    Vector3 closeMoveTarget = m_CharacterGameObject.transform.position + movementDirection*clampedDist;

                    // check if we have made it to our destination
                    float remainingDistance = (closeMoveTarget - m_CharacterGameObject.transform.position).magnitude;

                    if (remainingDistance < m_MinimumDistance) {
                        if (m_CurrentNavSegment == m_NavPath.corners.Length - 1)
                        {
                            m_LocomotionState = LocomotionState.TurnToTargetDirection;
                            m_CurrentSpeed = 0.0f;
                            m_VelocityXSmoothed = 0.0f;
                            m_VelocityZSmoothed = 0.0f;
                        }
                        else
                        {
                            ++m_CurrentNavSegment;
                            DecideMovementType();
                        }
                    } else {
                        // blend down speed when we are close to target

                        float timeTillTarget = remainingDistance/m_CurrentSpeed;
                        if (timeTillTarget < m_MinimumStoppingTime) {
                            m_CurrentSpeed -= m_Decceleration*Time.fixedDeltaTime;
                            if (m_CurrentSpeed < m_MinimumSpeed) {
                                m_CurrentSpeed = m_MinimumSpeed;
                            }
                        } else if (timeTillTarget < m_MinimumStoppingTime + 0.1f) {
                            // near around our slow down time, just coast
                        } else {
                            float additionalSpeed = m_Acceleration*Time.fixedDeltaTime;
                            // otherwise increase speed to max desired
                            m_CurrentSpeed += additionalSpeed;
                            if (m_CurrentSpeed > m_TargetSpeed) {
                                m_CurrentSpeed = m_TargetSpeed;
                            }
                        }

                        // get worldspace movement vector
                        Vector3 movementVelocity = movementDirection*m_CurrentSpeed;

                        // change vector to local space
                        movementVelocity = m_CharacterGameObject.transform.InverseTransformVector(movementVelocity);

                        // set animation parameters to try and move us to where we need to be
                        // actual object movement is controlled by the animation root motion
                        m_VelocityXSmoothed = movementVelocity.x;
                        m_VelocityZSmoothed = movementVelocity.z;
                        
                        // check if we want to turn to the target also
                        if (m_LocomotionState != LocomotionState.SideStepping) {
                            float angleOffset = Vector3.Angle(movementVelocity, Vector3.forward);
                            if (angleOffset > m_MinimumAngleDifference) {
                                m_TurnSmoothed = Mathf.Sign(movementVelocity.x)*angleOffset/45.0f;
                            } else {
                                m_TurnSmoothed = 0.0f;
                            }
                        }
                    }
                    break;
                }
                case LocomotionState.TurnToTargetDirection: {
                    // check if we are facing close enough to our target direction
                    Vector3 targetDirection = m_MoveTargetLocation.Rotation*Vector3.forward;
                    // change vector to local space
                    targetDirection = m_CharacterGameObject.transform.InverseTransformVector(targetDirection);

                    // check if we want to turn to the target also
                    float angleOffset = Vector3.Angle(targetDirection, Vector3.forward);
                    if (angleOffset > m_MinimumAngleDifference) {
                        m_TurnSmoothed = Mathf.Sign(targetDirection.x)*angleOffset/45.0f;
                    } else {
                        m_TurnSmoothed = 0.0f;
                        m_LocomotionState = LocomotionState.NotMoving;
                        m_Character.ResumeLookAt();
                    }
                    break;
                }
                    

            }
        }

        private void OnDrawGizmos()
        {
            if (m_NavPath != null)
            {
                for (int i = 0; i < m_NavPath.corners.Length - 1; ++i)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(m_NavPath.corners[i], m_NavPath.corners[i + 1]);
                }
            }

            if (m_MoveTargetLocation != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(m_MoveTargetLocation.Position, 0.15f);
            }
        }

        // private void OnGUI() {
        // 	// if (GUI.Button(new Rect(10, 10, 150, 50), "Move")) {
        // 	// 	MoveTo(m_DebugMoveTo.transform.position, 3.0f, true);
        // 	// }
        //     if (m_LocomotionState != LocomotionState.NotMoving)
        //     GUI.Label(new Rect(10, 100, 150, 50), m_LocomotionState.ToString());
        // }

    }
}