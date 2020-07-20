using System.Linq;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using StoryCore.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryCore {
    public class PerformanceTester : MonoBehaviour {
        [SerializeField, AutoFillAsset] private PerformanceBucket m_AnimBucket;
        [SerializeField] private bool m_Loop;
        [SerializeField] private TMP_Dropdown m_Dropdown;

        private int m_LastAnim = -1;
        private float m_NextTime;
        private Object[] m_AnimList;
        private Character[] m_Characters;

        public int Index { get; set; }
        public int Max => m_AnimList == null ? 0 : m_AnimList.Length;
        public bool Loop {
            get => m_Loop;
            set => m_Loop = value;
        }

        private Object Performance => m_AnimList[Index];

        public string PerformanceName => Performance ? Performance.name : "none";

        private void Start() {
            m_AnimList = m_AnimBucket.Items
                                     .Where(i => i is AnimationClip || i is CustomPerformance custom && custom.HasClip)
                                     .OrderBy(o => o.name)
                                     .ToArray();

            m_Characters = FindObjectsOfType<Character>();

            m_LastAnim = Index;
            m_Dropdown.options = m_AnimList
                                 .Where(o => o)
                                 .GroupBy(o => o.name)
                                 .Select(g => new TMP_Dropdown.OptionData(g.First().name))
                                 .ToList();
        }

        private void Update() {
            CheckAnim();
            CheckLoop();
        }

        private void CheckLoop() {
            if (!m_Loop) {
                return;
            }

            if (Time.time > m_NextTime && m_AnimList[Index]) {
                Play();
            }
        }

        private void CheckAnim() {
            if (Index == m_LastAnim) {
                return;
            }

            m_LastAnim = Index;

            if (m_AnimList == null || m_AnimList.Length == 0) {
                Debug.LogWarning("No valid animation found.");
                return;
            }

            m_LastAnim = Index = UnityUtils.Mod(Index, m_AnimList.Length);
            m_Dropdown.value = m_LastAnim;

            Play();
        }

        private void Play() {
            if (!Performance) {
                Debug.LogWarning("Clip number " + Index + " is missing.");
                return;
            }

            if (m_Characters == null || m_Characters.Length == 0) {
                Debug.LogWarning("No characters found. Can't play " + Performance.name);
                return;
            }

            m_NextTime = GetLength(Performance) + Time.time;

            foreach (Character character in m_Characters) {
                if (character) {
                    character.Play(Performance);
                }
            }
        }

        private float GetLength(Object performance) {
            if (performance is AnimationClip clip) {
                return clip.length;
            }

            if (performance is CustomPerformance custom) {
                return custom.Clips.First().length;
            }

            Debug.LogWarning("No clip found for " + performance.name, performance);
            return 0;
        }

        public void PreviousPerformance() {
            Index--;
        }

        public void NextPerformance() {
            Index++;
        }
    }
}