using System;
using CoreUtils;
using CoreUtils.AssetBuckets;
using StoryCore.Commands;
using CoreUtils.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class Globals : Singleton<Globals> {
        public static event Action Ready;

        [SerializeField] private CommandHandler m_CommandRecenter;
        [SerializeField] private GameVariableGameObject m_RecenterTarget;
        [SerializeField] private GameVariableSpawnPoint m_CurrentSpawnPoint;
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private GameVariableBool m_IsJournalOpen;
        [SerializeField, AutoFillAsset] private VOBucket m_VO;
        [SerializeField, AutoFillAsset] private TextReplacementConfig m_TextReplacementConfig; 

        public static CommandHandler CommandRecenter => Exists ? Instance.m_CommandRecenter : null;
        public static GameVariableGameObject RecenterTarget => Exists ? Instance.m_RecenterTarget : null;
        public static GameVariableSpawnPoint CurrentSpawnPoint => Exists ? Instance.m_CurrentSpawnPoint : null;
        public static StoryTeller StoryTeller => Exists ? Instance.m_StoryTeller : null;
        public static GameVariableBool IsJournalOpen => Exists ? Instance.m_IsJournalOpen : null;
        public static VOBucket VO => Exists ? Instance.m_VO : null;
        public static TextReplacementConfig TextReplacementConfig => Exists ? Instance.m_TextReplacementConfig : null;

        private void Awake() {
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