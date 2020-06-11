using System;
using StoryCore;
using StoryCore.Utils;
using UnityEngine;

namespace VRTK {
    public class StoryCoreSettings : MonoBehaviour {

        #region Global Settings

        private static string kUseDebugInInkKey = "StoryCore-UseDebugInInk";
        private static string kForceSimulationMode = "StoryCore-ForceSimulationMode";

        private static int s_UseDebugInInk = -1;
        private static int s_ForceSimulationMode = -1;

        public static bool UseDebugInInk {
            get {
                if (s_UseDebugInInk < 0) {
                    s_UseDebugInInk = PlayerPrefs.GetInt(kUseDebugInInkKey);
                }

                return s_UseDebugInInk > 0;
            }
            set {
                if (s_UseDebugInInk >= 0 && s_UseDebugInInk > 0 == value) {
                    return;
                }

                s_UseDebugInInk = value ? 1 : 0;
                PlayerPrefs.SetInt(kUseDebugInInkKey, s_UseDebugInInk);
            }
        }

        public static bool ForceSimulationMode {
            get {
                if (s_ForceSimulationMode < 0) {
                    s_ForceSimulationMode = PlayerPrefs.GetInt(kForceSimulationMode);
                }

                return s_ForceSimulationMode > 0;
            }
            set {
                if (s_ForceSimulationMode >= 0 && s_ForceSimulationMode > 0 == value) {
                    return;
                }

                s_ForceSimulationMode = value ? 1 : 0;
                PlayerPrefs.SetInt(kForceSimulationMode, s_ForceSimulationMode);
            }
        }

        #endregion

        [SerializeField] private VRTK_SDKManager m_Manager;
        [SerializeField] private StoryTeller m_StoryTeller;

#if UNITY_EDITOR
        public VRTK_SDKManager Manager {
            get => m_Manager;
            set => m_Manager = value;
        }

        public StoryTeller StoryTeller {
            get => m_StoryTeller;
            set => m_StoryTeller = value;
        }

        private void OnEnable() {
            if (!Application.isEditor) {
                return;
            }

            CheckForceSimulationMode();
            CheckIsDebug();
        }

        private void CheckIsDebug() {
            if (StoryTeller) {
                StoryTeller.SetDebugInScript(UseDebugInInk);
            }
        }

        private void CheckForceSimulationMode() {
            if (!ForceSimulationMode) {
                return;
            }

            Delay.ForFrameCount(10, this).Then(SetForceSimulationMode);
        }

        private void SetForceSimulationMode() {
            int setupIndex = Manager.setups.IndexOf(s => s.name.Equals("Simulator", StringComparison.OrdinalIgnoreCase));

            if (setupIndex >= 0 && Manager) {
                Debug.Log("Forcing VR Simulator Mode (Mouse & Keyboard)");
                Manager.TryLoadSDKSetup(setupIndex, true, Manager.setups);
            } else {
                Debug.LogWarning("Couldn't force VR Simulator Mode. 'Simulator' not found.");
            }
        }

        private void Reset() {
            Manager = FindObjectOfType<VRTK_SDKManager>();
            StoryTeller = FindObjectOfType<StoryTeller>();
        }
#endif
    }
}