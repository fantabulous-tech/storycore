using System;
using StoryCore.AssetBuckets;
using StoryCore.Commands;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class Globals : MonoBehaviour {
        private static Globals s_Instance;
        public static event Action Ready;

        [SerializeField] private CommandHandler m_CommandRecenter;
        [SerializeField] private GameVariableGameObject m_RecenterTarget;
        [SerializeField] private GameVariableSpawnPoint m_CurrentSpawnPoint;
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private GameVariableBool m_IsJournalOpen;
        [SerializeField, AutoFillAsset] private VOBucket m_VO;

        public static bool IsActive => s_Instance;
        public static CommandHandler CommandRecenter => s_Instance ? s_Instance.m_CommandRecenter : null;
        public static GameVariableGameObject RecenterTarget => s_Instance ? s_Instance.m_RecenterTarget : null;
        public static GameVariableSpawnPoint CurrentSpawnPoint => s_Instance ? s_Instance.m_CurrentSpawnPoint : null;
        public static StoryTeller StoryTeller => s_Instance ? s_Instance.m_StoryTeller : null;
        public static GameVariableBool IsJournalOpen => s_Instance ? s_Instance.m_IsJournalOpen : null;
        public static VOBucket VO => s_Instance ? s_Instance.m_VO : null;

        private void Awake() {
            s_Instance = this;
            VRTK_SDKManager.instance.LoadedSetupChanged += (s, e) => UpdateCamera();
            UpdateCamera();
            Ready?.Invoke();
            Ready = null;
        }

        private static void UpdateCamera() {
            Transform camTransform = VRTK_DeviceFinder.HeadsetCamera();
            Camera cam = camTransform ? camTransform.GetComponent<Camera>() : null;

            if (cam) {
                UnityUtils.SetUtilCamera(cam);
            }
        }
    }
}