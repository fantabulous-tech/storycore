using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class RandomChange : MonoBehaviour {
        [SerializeField] private float m_MinWait = 5;
        [SerializeField] private float m_MaxWait = 10;

        private Animator m_Animator;
        private static readonly int s_Change = Animator.StringToHash("Change");

        private void Start() {
            m_Animator = GetComponent<Animator>();

            if (Random.value > 0.5f) {
                Change();
            } else {
                Delay.For(Random.Range(m_MinWait, m_MaxWait), this).Then(Change);
            }
        }

        private void Change() {
            m_Animator.SetTrigger(s_Change);
            Delay.For(Random.Range(m_MinWait, m_MaxWait), this).Then(Change);
        }
    }
}