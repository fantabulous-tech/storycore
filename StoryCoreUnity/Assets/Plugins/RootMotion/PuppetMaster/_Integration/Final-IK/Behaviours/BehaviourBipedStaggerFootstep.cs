using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {
	
	/// <summary>
	/// This is just a commented template for creating new Puppet Behaviours.
	/// </summary>
	public partial class BehaviourBipedStagger : BehaviourBase {

		[System.Serializable]
		public class Footstep {
			
			public IKEffector effector { get; private set; }
			public Transform thigh { get; private set; }
			public Muscle pelvisMuscle;
			public bool isStepping { get { return stepProgress < 1f; }}
			public bool isLeft { get; private set; }
			public float stepProgress { get; private set; }
			
			public Footstep (IKEffector effector, Transform thigh, Muscle pelvisMuscle, bool isLeft) {
				this.effector = effector;
				this.thigh = thigh;
				this.pelvisMuscle = pelvisMuscle;
				this.isLeft = isLeft;
				
				Reset();
			}
			
			public void Reset() {
				stepProgress = 1f;
			}
			
			public void StartStep() {
				stepProgress = 0f;
			}
			
			public void Update(float deltaTime, float speed, Quaternion rotationOffset, float magnitude, AnimationCurve weightByProgress, float weight) {
				if (!isStepping) return;
				
				float w = weightByProgress.Evaluate(stepProgress) * weight;
				
				stepProgress = Mathf.MoveTowards(stepProgress, 1f, deltaTime * speed);
				
				Vector3 p = effector.bone.position + Vector3.up * magnitude;
				p = thigh.position + rotationOffset * (p - thigh.position);
				
				//Quaternion footRotationOffset = rotationOffset;// * rotationOffset;
				//effector.bone.rotation = Quaternion.Lerp(Quaternion.identity, footRotationOffset, weight) * effector.bone.rotation;
				
				effector.positionOffset += (p - effector.bone.position) * w;
			}
		}
	}
}