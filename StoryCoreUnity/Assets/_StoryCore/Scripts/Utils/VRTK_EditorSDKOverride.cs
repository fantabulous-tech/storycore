using StoryCore.Utils;
using UnityEngine;

namespace VRTK {
    public class VRTK_EditorSDKOverride : MonoBehaviour {
        [SerializeField] private VRTK_SDKManager m_Manager;
        [SerializeField] private GameObject m_SetupGroup;
        [SerializeField] private int m_Index;

        public int Index {
            get => m_Index;
            set => m_Index = value;
        }
        public VRTK_SDKManager Manager => m_Manager;

        public VRTK_SDKSetup[] Setups => m_Manager ? m_Manager.setups : new VRTK_SDKSetup[] { };

        private void Start() {
            Delay.ForFrameCount(10, this).Then(LoadSDKSetup);
        }

        public void LoadSDKSetup() {
            // 'Index - 1' because 'Index' = list of setups + 'none' at index '0'.
            int setupIndex = Index - 1;

            if (setupIndex >= 0 && m_Manager && Application.isEditor) {
                m_Manager.TryLoadSDKSetup(setupIndex, true, m_Manager.setups);
                if (m_SetupGroup) {
                    m_SetupGroup.SetActive(true);
                }
            }
        }

        private void Reset() {
            m_Manager = GetComponent<VRTK_SDKManager>();
            if (m_Manager) {
                Index = m_Manager.setups.Length;
            }
        }
    }
}