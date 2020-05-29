using System;
using UnityEngine;

namespace StoryCore.PoseTracking {
    public class VRHandOffset : MonoBehaviour {
        private static Transform s_RightHandOffset;
        private static Transform s_LeftHandOffset;

        public static Transform GetHand(Hand hand) {
            switch (hand) {
                case Hand.Right:
                    return s_RightHandOffset;
                case Hand.Left:
                    return s_LeftHandOffset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hand), hand, null);
            }
        }

        [SerializeField] private Hand m_Hand;

        public enum Hand {
            Right,
            Left
        }

        private void Awake() {
            switch (m_Hand) {
                case Hand.Right:
                    Debug.Assert(!s_RightHandOffset, this);
                    s_RightHandOffset = transform;
                    break;
                case Hand.Left:
                    Debug.Assert(!s_LeftHandOffset, this);
                    s_LeftHandOffset = transform;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}