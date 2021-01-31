using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {
	
	/// <summary>
	/// This is just a commented template for creating new Puppet Behaviours.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourBipedStagger")]
	public partial class BehaviourBipedStagger : BehaviourBase {

        protected override string GetTypeSpring()
        {
            return typeSpring;
        }
        
        private const string typeSpring = "BehaviourBipedStagger";

        [LargeHeader("Balancers")]

		[Tooltip("The layers to stagger on.")]
		/// <summary>
		/// The layers to stagger on.
		/// </summary>
		public LayerMask groundLayers;

		[Tooltip("Settings for the balancer. Balancers are using ankle muscles to try to keep the puppet balanced with no external forces.")]
		/// <summary>
		/// Settings for the balancer.
		/// </summary>
		public SubBehaviourBalancer.Settings balancerSettings;

		[LargeHeader("Main Properties")]

		[Tooltip("Maximum duration for the staggering.")]
		/// <summary>
		/// Maximum duration for the staggering.
		/// </summary>
		public float maxDuration = 3f;

		[Tooltip("Time from losing balance to the 'On Finished' event.")]
		/// <summary>
		/// Time from losing balance to the 'On Finished' event.
		/// </summary>
		public float finishTime = 1f;

		[Tooltip("Muscle weight will be set to this value when balance is lost.")]
		/// <summary>
		/// Muscle weight will be set to this value when balance is lost.
		/// </summary>
		public float unbalancedMuscleWeightMlp = 0.5f;

		[LargeHeader("Legs")]

		[Tooltip("Min angle of the COM (Center Of Mass) vector to start stepping.")]
		/// <summary>
		/// Min angle of the COM (Center Of Mass) vector to start stepping.
		/// </summary>
		public float minAngle = 15f;

		[Tooltip("Max angle of the COM (Center Of Mass) vector, after which the puppet is considered to have hopelessly lost balance.")]
		/// <summary>
		/// Max angle of the COM (Center Of Mass) vector, after which the puppet is considered to have hopelessly lost balance.
		/// </summary>
		public float maxAngle = 75f;

		[Tooltip("The height of footsteps.")]
		/// <summary>
		/// The height of footsteps.
		/// </summary>
		public float stepHeight = 0.1f;

		[Tooltip("Increase this value to make the puppet take larger steps when moving faster.")]
		/// <summary>
		/// Increase this value to make the puppet take larger steps when moving faster.
		/// </summary>
		public float stepRotationVFactor;

		[Tooltip("The general speed multiplier for staggering.")]
		/// <summary>
		/// The general speed multiplier for staggering.
		/// </summary>
		public float speedMlp = 1f;

		[Tooltip("The speed of staggering evaluated by the angle of the COM vector. Lift the end of the curve to make staggering faster when the puppet is about to fall over.")]
		/// <summary>
		/// The speed of staggering evaluated by the angle of the COM vector. Lift the end of the curve to make staggering faster when the puppet is about to fall over.
		/// </summary>
		public AnimationCurve speedByAngle;

		[Tooltip("Weight curve for the footstep evaluated by normalized (0 - 1) progress of the step. Lift the start point of the curve to make footsteps have more punch at the start.")]
		/// <summary>
		/// Weight curve for the footstep evaluated by normalized (0 - 1) progress of the step. Lift the start point of the curve to make footsteps have more punch at the start.
		/// </summary>
		public AnimationCurve weightByProgress;

		[Tooltip("Lift the last keyframe curve to increase step height when the puppet is staggering in forward direction. Lift the first keyframe to do the same in backward direction.")]
		/// <summary>
		/// Lift the last keyframe curve to increase step height when the puppet is staggering in forward direction. Lift the first keyframe to do the same in backward direction.
		/// </summary>
		public AnimationCurve stepHeightmlpByProneDot;

		[Tooltip("Lift the last keyframe to make the puppet lose balance more before starting to stagger in forward direction. Lift the first keyframe to do the same in backward direction.")]
		/// <summary>
		/// Lift the last keyframe to make the puppet lose balance more before starting to stagger in forward direction. Lift the first keyframe to do the same in backward direction.
		/// </summary>
		public AnimationCurve minAngleMlpByProneDot;
		
		[LargeHeader("Arms")]

		/// <summary>
		/// The general weight of arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.
		/// </summary>
		public Weight windmillWeight = new Weight(1f, "The general weight of arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.");

		/// <summary>
		/// Muscle weight of the arms. If using a curve, the value is evaluated by the angle of the COM vector.
		/// </summary>
		public Weight muscleWeight = new Weight(1f, "Muscle weight of the arms. If using a curve, the value is evaluated by the angle of the COM vector.");

		/// <summary>
		/// The speed of the windmill. If using a curve, the value is evaluated by the angle of the COM vector.
		/// </summary>
		public Weight windmillSpeed = new Weight(12f, "The speed of the windmill. If using a curve, the value is evaluated by the angle of the COM vector.");

		/// <summary>
		/// The spread of the arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.
		/// </summary>
		public Weight windmillSpread = new Weight(0.8f, "The spread of the arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.");

		/// <summary>
		/// The radius of the arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.
		/// </summary>
		public Weight windmillRadius = new Weight(0.5f, "The radius of the arm windmill. If using a curve, the value is evaluated by the angle of the COM vector.");

		[Tooltip("Overrides 'Maintain Relative Pos' value for the arms in the FullBodyBipedIK component.")]
		/// <summary>
		/// Overrides 'Maintain Relative Pos' value for the arms in the FullBodyBipedIK component.
		/// </summary>
		[Range(0f, 1f)] public float maintainArmRelativePos = 0.5f;

		[Tooltip("0 keeps the left and right arm in sync, 0.5 has the left arm pointing up while the right is pointing down.")]
		/// <summary>
		/// 0 keeps the left and right arm in sync, 0.5 has the left arm pointing up while the right is pointing down.
		/// </summary>
		[Range(0f, 1f)] public float windmillSyncOffset = 0.3f;

		[Tooltip("Offset of the hand targets in character space.")]
		/// <summary>
		/// Offset of the hand targets in character space.
		/// </summary>
		public Vector3 windmillOffset;

		[LargeHeader("Spine")]

		[Tooltip("Muscle weight multiplier for all spine bones.")]
		/// <summary>
		/// Muscle weight multiplier for all spine bones.
		/// </summary>
		public float spineMuscleWeightMlp = 0.1f;

		[LargeHeader("Events")]

		[Tooltip("Called when the behaviour is activated.")]
		/// <summary>
		/// Called when the behaviour is activated.
		/// </summary>
		public PuppetEvent onActivate;

		[Tooltip("Called when the puppet has lost balance and stopped staggering.")]
		/// <summary>
		/// Called when the puppet has lost balance and stopped staggering.
		/// </summary>
		public PuppetEvent onLoseBalance;

		[Tooltip("Called when the behaviour is finished (after 'Finish Time' has passed from the 'On Lose Balance' event.")]
		/// <summary>
		/// Called when the behaviour is finished (after 'Finish Time' has passed from the 'On Lose Balance' event.
		/// </summary>
		public PuppetEvent onFinished;

		public bool forceFinish { get; private set; }

		private FullBodyBipedIK ik;
		private Rigidbody[] rigidbodies = new Rigidbody[0];
		private SubBehaviourBalancer[] balancers = new SubBehaviourBalancer[0];
		private float weight = 1f;
		private Vector3 dirLeftSmooth;
		private Vector3 dirRightSmooth;
		private float lastTime;
		private Muscle pelvisMuscle;
		private float progress;
		private Vector3 pelvisForward;
		private Footstep[] footsteps = new Footstep[0];
		private int lastStepLegIndex = -1;
		private Vector3 dirSmooth;
		private bool isFinished;
		private bool balanceLost;
		private float finishTimer;
		private float windmillAngle;
		private Vector3 com, comV, dir, dirVel, dirHorVel;
		private float deltaTime, angle, angleVel, proneDot, mlp;
		private float timer;

		public void Finish() {
			if (!enabled) return;
			forceFinish = true;
		}

		// Initiate something. This is called only once by the PuppetMaster in Start().
		protected override void OnInitiate() {
			ik = puppetMaster.targetRoot.GetComponentInChildren<FullBodyBipedIK>();
			if (ik == null) {
				Debug.LogError("No FullBodyBipedIK component found on the target hierarchy, can not initiate BehaviourBipedStagger.", transform);
				enabled = false;
				return;
			}
			if (ik.solver.iterations > 0) Debug.LogWarning("BehaviourBipedStagger works best and fastest with FullBodyBipedIK solver iterations set to 0.", transform);

			ik.solver.OnPreUpdate += Solve;

			// Balancers
			Transform[] copPoints = new Transform[0];
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Foot) {
					System.Array.Resize(ref copPoints, copPoints.Length + 1);
					copPoints[copPoints.Length - 1] = m.transform;
				}
			}

			if (copPoints.Length == 0) {
				Debug.LogError("No 'Foot' muscles found, please assign a Group for each muscle in PuppetMaster.", transform);
				enabled = false;
				return;
			}

			rigidbodies = new Rigidbody[puppetMaster.muscles.Length];
			for (int i = 0; i < rigidbodies.Length; i++) {
				rigidbodies[i] = puppetMaster.muscles[i].rigidbody;
			}

			// TODO Check for rigidbody mass
			// TODO Make sure Angular Limits is enabled

			foreach (Muscle m in puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Foot) {
					System.Array.Resize(ref balancers, balancers.Length + 1);
					balancers[balancers.Length - 1] = new SubBehaviourBalancer();

					balancers[balancers.Length - 1].Initiate(this as BehaviourBase, balancerSettings, m.joint.connectedBody, rigidbodies, m.joint, copPoints, m.joint.GetComponent<PressureSensor>());

					PressureSensor pressureSensor = m.transform.gameObject.AddComponent<PressureSensor>();
					pressureSensor.layers = groundLayers;
				}
			}

			// Muscle axes
			pelvisMuscle = puppetMaster.muscles[0];
			pelvisForward = Quaternion.Inverse(pelvisMuscle.target.rotation) * transform.forward;

			// Footsteps
			footsteps = new Footstep[2] { 
				new Footstep(ik.solver.leftFootEffector, ik.references.leftThigh, pelvisMuscle, true), 
				new Footstep(ik.solver.rightFootEffector, ik.references.rightThigh, pelvisMuscle, false) 
			};

			// IK settings
			ik.solver.leftLegMapping.maintainRotationWeight = 0f;
			ik.solver.rightLegMapping.maintainRotationWeight = 0f;
		}

		private void Solve() {
			if (!enabled) return;

			deltaTime = Time.time - lastTime;
			lastTime = Time.time;

			com = Vector3.zero;
			foreach (SubBehaviourBalancer b in balancers) com += b.com;
			com /= balancers.Length;

			comV = Vector3.zero;
			foreach (SubBehaviourBalancer b in balancers) comV += b.comV;
			comV /= balancers.Length;

			dir = Vector3.zero;
			foreach (SubBehaviourBalancer b in balancers) dir += b.dir;
			dir /= balancers.Length;

			dirVel = Vector3.zero;
			foreach (SubBehaviourBalancer b in balancers) dirVel += b.dirVel;
			dirVel /= balancers.Length;

			dirHorVel = new Vector3(dirVel.x, 0f, dirVel.z);
			angle = Vector3.Angle(dir, Vector3.up);
			angleVel = Vector3.Angle(dirVel, Vector3.up);
			proneDot = Vector3.Dot(pelvisMuscle.transform.rotation * pelvisForward, dirHorVel);
			mlp = stepHeightmlpByProneDot.Evaluate(proneDot);
			
			SolveLegs(Time.deltaTime);
			SolveArms(Time.deltaTime);
		}

		#region Legs
		
		private void SolveLegs(float deltaTime) {
			// TODO Use LineSphereCollision to select next leg/offset stepping direction
			
			if (!isFinished) {
				int nextStepLegIndex = GetNextStepLegIndex(deltaTime);
				
				if (nextStepLegIndex != -1) {
					footsteps[nextStepLegIndex].StartStep();
					lastStepLegIndex = nextStepLegIndex;
				}
				
				/*
				Quaternion r = Quaternion.FromToRotation(dir, Vector3.up);
				float ang = 0;
				Vector3 axis = Vector3.zero;
				r.ToAngleAxis(out ang, out axis);
				if (ang < 0) axis = -axis;
				ang = 1f;
				r = Quaternion.AngleAxis(ang * angle * directionMag, axis);
				*/
			}

			if (isFinished || balanceLost) weight = Mathf.MoveTowards(weight, 0f, deltaTime * 5f);

			Quaternion r = Quaternion.FromToRotation(dir + dirVel * stepRotationVFactor, Vector3.up);
			//Quaternion r = Quaternion.FromToRotation(Vector3.Lerp(dir, dirVel, stepRotationVFactor), Vector3.up);
			
			float speed = speedByAngle.Evaluate(angle) * speedMlp;
			
			foreach (Footstep footstep in footsteps) {
				footstep.Update(deltaTime, speed, RotationToAnimationSpace(r), stepHeight * mlp, weightByProgress, weight);
			}
		}
		
		private int GetNextStepLegIndex(float deltaTime) {
			// is stepping
			if (angleVel < minAngle * minAngleMlpByProneDot.Evaluate(proneDot) || angle > maxAngle || timer >= maxDuration || forceFinish) {
				lastStepLegIndex = -1;
				
				if (!isFinished && angle > maxAngle || timer >= maxDuration || forceFinish) {
					finishTimer += deltaTime;

					if (!balanceLost && finishTimer > 0.1f) {
						balanceLost = true;

						foreach (Muscle m in puppetMaster.muscles) {
							m.state.muscleWeightMlp = unbalancedMuscleWeightMlp;
						}

						onLoseBalance.Trigger(puppetMaster);
						if (onLoseBalance.switchBehaviour) return -1;
					}

					if (finishTimer >= finishTime) {
						isFinished = true;
						finishTimer = 0f;

						onFinished.Trigger(puppetMaster);
						if (onFinished.switchBehaviour) return -1;
						enabled = false;
					}

					if (balanceLost) return -1;
				} else finishTimer = 0f;
				
				return -1;
			} else finishTimer = 0f;
			
			if (!CanStep()) return -1;
			
			// If first step, step with the foot that is closest to the com direction
			if (lastStepLegIndex == -1) {
				Vector3 p = com + comV + dirVel;
				
				float leftDist = Vector3.Distance(p, puppetMaster.GetMuscle(ik.references.leftFoot).transform.position);
				float rightDist = Vector3.Distance(p, puppetMaster.GetMuscle(ik.references.rightFoot).transform.position);
				if (leftDist < rightDist) return 1;
				return 0;
			}
			
			int next = lastStepLegIndex + 1;
			if (next > 1) next = 0;
			return next;
		}
		
		private bool CanStep() {
			foreach (Footstep f in footsteps) if (f.isStepping && f.stepProgress < 0.8) return false;
			
			return true;
		}
		
		#endregion Legs

		#region Arms
		
		private void SolveArms(float deltaTime) {
			float mW = muscleWeight.GetValue(angle);
			
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Arm || m.props.group == Muscle.Group.Hand) m.state.muscleWeightMlp = mW;
			}
			
			// Chest
			Vector3 shoulderPosLeft = ik.solver.leftShoulderEffector.bone.position + ik.solver.leftShoulderEffector.positionOffset;
			Vector3 shoulderPosRight = ik.solver.rightShoulderEffector.bone.position + ik.solver.rightShoulderEffector.positionOffset;
			Vector3 shoulderDirection = (shoulderPosLeft - shoulderPosRight).normalized;
			Vector3 up = puppetMaster.targetRoot.up;
			Vector3 chestForward = Vector3.Cross(up, shoulderDirection);
			float armLengthLeft = ik.solver.leftArmChain.nodes[0].length + ik.solver.leftArmChain.nodes[1].length;
			float armLengthRight = ik.solver.rightArmChain.nodes[0].length + ik.solver.rightArmChain.nodes[1].length;
			
			windmillAngle += deltaTime * windmillSpeed.GetValue(angle) * Mathf.Rad2Deg;
			if (windmillAngle > 360f) windmillAngle -= 360f;
			if (windmillAngle < -360f) windmillAngle += 360f;
			
			float w = windmillWeight.GetValue(angle);
			
			Vector3 leftHandPos = GetArmPositionWindmill(shoulderPosLeft, shoulderDirection, chestForward, armLengthLeft, windmillAngle);
			Vector3 rightHandPos = GetArmPositionWindmill(shoulderPosRight, -shoulderDirection, chestForward, armLengthRight, -windmillAngle + windmillSyncOffset * 306f);
			
			ik.references.leftUpperArm.parent.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(ik.references.leftHand.position - ik.references.leftUpperArm.parent.position, leftHandPos - ik.references.leftUpperArm.parent.position), w * 0.25f) * ik.references.leftUpperArm.parent.rotation;
			ik.references.rightUpperArm.parent.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(ik.references.rightHand.position - ik.references.rightUpperArm.parent.position, rightHandPos - ik.references.rightUpperArm.parent.position), w * 0.25f) * ik.references.rightUpperArm.parent.rotation;
			
			ik.references.leftUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(ik.references.leftHand.position - ik.references.leftUpperArm.position, leftHandPos - ik.references.leftUpperArm.position), w) * ik.references.leftUpperArm.rotation;
			ik.references.rightUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(ik.references.rightHand.position - ik.references.rightUpperArm.position, rightHandPos - ik.references.rightUpperArm.position), w) * ik.references.rightUpperArm.rotation;
			
			//ik.solver.leftHandEffector.positionOffset += (leftHandPos - ik.solver.leftHandEffector.bone.position) * w;
			//ik.solver.rightHandEffector.positionOffset += (rightHandPos - ik.solver.rightHandEffector.bone.position) * w;
			
			// Maintain Relative Position Weight
			ik.solver.leftHandEffector.maintainRelativePositionWeight = Mathf.Max(maintainArmRelativePos - w, 0f); // @todo reset maintainRelativePositionWeight in OnDisable
			ik.solver.rightHandEffector.maintainRelativePositionWeight = Mathf.Max(maintainArmRelativePos - w, 0f);
		}
		
		private Vector3 GetArmPositionWindmill(Vector3 shoulderPosition, Vector3 shoulderDirection, Vector3 chestForward, float armLength, float windmillAngle) {
			Quaternion chestRotation = Quaternion.LookRotation(chestForward, puppetMaster.targetRoot.up);
			
			Vector3 toSide = shoulderDirection * armLength * windmillSpread.GetValue(angle);
			
			Quaternion windmillRotation = Quaternion.AngleAxis(windmillAngle, shoulderDirection);
			
			Vector3 toWindmill = windmillRotation * chestForward * armLength * windmillRadius.GetValue(angle);
			Vector3 windmillPos = shoulderPosition + toSide + toWindmill;
			windmillPos += chestRotation * windmillOffset;
			
			return windmillPos;
		}
		
		#endregion Arms
		
		protected override void OnActivate() {
			// When this becomes the active behaviour. There can only be one active behaviour. 
			// Switching behaviours is done by the behaviours themselves, using PuppetEvents.
			// Each behaviour should know when it is no longer required and which behaviours to switch to in each case.
			isFinished = false;
			balanceLost = false;
			finishTimer = 0f;
			weight = 0f;
			timer = 0f;
			forceFinish = false;

			foreach (Muscle m in puppetMaster.muscles) {
				m.state.pinWeightMlp = 0f;
				m.state.muscleWeightMlp = 1f;

				if (m.props.group == Muscle.Group.Foot) {
					PressureSensor pressureSensor = m.transform.GetComponent<PressureSensor>();
					if (pressureSensor == null) pressureSensor = m.transform.gameObject.AddComponent<PressureSensor>();
					pressureSensor.layers = groundLayers;
				} else if (m.props.group == Muscle.Group.Spine || m.props.group == Muscle.Group.Head) {
					m.state.muscleWeightMlp = spineMuscleWeightMlp;
				}
			}

			onActivate.Trigger(puppetMaster);
		}
		
		public override void OnReactivate() {
			// Called when the PuppetMaster has been deactivated (by parenting it to an inactive hierarchy or calling SetActive(false)).
		}
		
		protected override void OnDeactivate() {
			// Called when this behaviour is exited. OnActivate is the place for resetting variables to defaults though.
			isFinished = false;
			balanceLost = false;
			finishTimer = 0f;
			timer = 0f;
			forceFinish = false;

			foreach (Muscle m in puppetMaster.muscles) {
				m.state.muscleWeightMlp = 1f;
				m.state.mappingWeightMlp = 1f;

				if (m.props.group == Muscle.Group.Foot) {
					m.state.muscleDamperMlp = 1f;
					m.state.muscleDamperAdd = 0f;
					m.state.maxForceMlp = 1f;

					PressureSensor pressureSensor = m.transform.GetComponent<PressureSensor>();
					if (pressureSensor != null) Destroy(pressureSensor);
				}
			}

			foreach (SubBehaviourBalancer balancer in balancers) {
				balancer.joint.targetAngularVelocity = Vector3.zero;
			}
		}
		
		protected override void OnFixedUpdate(float deltaTime) {
			if (!balanceLost) weight = Mathf.MoveTowards(weight, 1f, deltaTime * 5f);

			timer += deltaTime;

			foreach (Muscle m in puppetMaster.muscles) {
				m.state.mappingWeightMlp = Mathf.MoveTowards(m.state.mappingWeightMlp, 1f, deltaTime * 5f);

				if (m.props.group == Muscle.Group.Foot) {
					m.state.muscleDamperMlp = 0f;
					m.state.muscleDamperAdd = m.props.muscleWeight * puppetMaster.muscleSpring * balancerSettings.damperForSpring;
					m.state.maxForceMlp = balancerSettings.maxForceMlp;
				}
			}
		}
		
		protected override void OnLateUpdate(float deltaTime) {
			// Everything happening in LateUpdate().
		}
		
		protected override void OnMuscleHitBehaviour(MuscleHit hit) {
			if (!enabled) return;
			
			// If the muscle has been hit via code using MuscleCollisionBroadcaster.Hit(float unPin, Vector3 force, Vector3 position);
			// This is used for shooting based on raycasting instead of physical collisions.
		}
		
		protected override void OnMuscleCollisionBehaviour(MuscleCollision m) {
			if (!enabled) return;
			
			// If the muscle has collided with something that is on the PuppetMaster's collision layers.
		}

		private Vector3 PointToAnimationSpace(Vector3 worldSpacePosition) {
			Vector3 relativeToMuscle = Quaternion.Inverse(pelvisMuscle.transform.rotation) * (worldSpacePosition - pelvisMuscle.transform.position);
			Vector3 p = pelvisMuscle.target.position + pelvisMuscle.target.rotation * relativeToMuscle;
			
			return p;
		}
		
		private Vector3 VectorToAnimationSpace(Vector3 worldSpaceV) {
			Vector3 relativeToMuscle = Quaternion.Inverse(pelvisMuscle.transform.rotation) * worldSpaceV;
			Vector3 p = pelvisMuscle.target.rotation * relativeToMuscle;
			
			return p;
		}
		
		private Quaternion RotationToAnimationSpace(Quaternion worldSpaceRotation) {
			Quaternion relativeToMuscle = Quaternion.Inverse(pelvisMuscle.transform.rotation) * worldSpaceRotation;
			return pelvisMuscle.target.rotation * relativeToMuscle;
		}
		
		private static bool GetLineSphereCollision(Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter, float sphereRadius) {
			Vector3 line = lineEnd - lineStart;
			Vector3 toSphere = sphereCenter - lineStart;
			float distToSphereCenter = toSphere.magnitude;
			float d = distToSphereCenter - sphereRadius;
			
			if (d > line.magnitude) return false;
			
			Quaternion q = Quaternion.LookRotation(line, toSphere);
			
			Vector3 toSphereRotated = Quaternion.Inverse(q) * toSphere;
			
			if (toSphereRotated.z < 0f) {
				return d < 0f; 
			} 
			
			return toSphereRotated.y - sphereRadius < 0f;
		}
		
	}
}