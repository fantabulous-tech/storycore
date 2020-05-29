using UnityEngine;

namespace StoryCore.Utils {
    [ExecuteInEditMode]
    public class ForceAnimState : MonoBehaviour {
        [SerializeField] private AnimationClip m_Animation;
        [SerializeField] private float m_Time;

        private void LateUpdate() {
            if (m_Animation) {
                m_Animation.SampleAnimation(gameObject, MathUtils.Mod(m_Time, m_Animation.length));
            }
        }
    }
}