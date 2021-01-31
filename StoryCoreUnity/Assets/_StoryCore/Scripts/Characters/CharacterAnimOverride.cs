using UnityEngine;

namespace StoryCore.Characters {
    public class CharacterAnimOverride : MonoBehaviour {
        [SerializeField] private AnimationClip m_OverrideClip;
        [SerializeField] private AvatarMask m_Mask;

        public AnimationClip OverrideClip => m_OverrideClip;
        public AvatarMask Mask => m_Mask;
    }
}