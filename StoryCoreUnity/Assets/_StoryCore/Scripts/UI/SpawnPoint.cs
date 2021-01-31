using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class SpawnPoint : MonoBehaviour {
        [SerializeField] private bool m_UseHeadsetOffset = true;

        private bool m_Init;

        protected void Awake() {
            if (Globals.Exists) {
                Globals.CommandRecenter.GenericEvent += OnRecenter;
            } else {
                Globals.Ready += OnGlobalsReady;
            }
        }

        private void OnEnable() {
            CheckInit();
        }

        private void Update() {
            CheckInit();
        }

        private void OnDestroy() {
            if (!AppTracker.IsQuitting) {
                if (Globals.Exists) {
                    Globals.CommandRecenter.GenericEvent -= OnRecenter;
                }
                if (VRTK_SDKManager.instance != null) {
                    VRTK_SDKManager.instance.LoadedSetupChanged -= OnLoadedSetupChanged;
                }
            }
        }

        private void CheckInit() {
            if (m_Init || !VRTK_SDKManager.instance) {
                return;
            }

            VRTK_SDKSetup setup = VRTK_SDKManager.GetLoadedSDKSetup();

            if (setup) {
                Teleport();
            }

            VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;
            m_Init = true;
        }

        private void OnGlobalsReady() {
            Globals.CommandRecenter.GenericEvent += OnRecenter;
        }

        private void OnRecenter() {
            if (Globals.RecenterTarget.Value == gameObject) {
                Teleport();
            }
        }

        private void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            if (e.currentSetup) {
                Teleport();
            }
        }

        public void Teleport() {
            if (Globals.CurrentSpawnPoint != null) {
                Globals.CurrentSpawnPoint.Value = this;
            }
            VRUtils.Teleport(transform, m_UseHeadsetOffset);
        }
    }
}