using System.Collections.Generic;
using StoryCore.Utils;
using UnityEngine;

namespace CoreUtils {
    public class SimulatorOverrides : MonoBehaviour {
        [SerializeField] private GameObject[] m_DisableInSimulatorMode;
        [SerializeField] private GameObject[] m_EnableInSimulatorMode;

        private readonly List<GameObject> m_DisabledGameObjects = new List<GameObject>();
        private readonly List<GameObject> m_EnabledGameObjects = new List<GameObject>();

        private void OnEnable() {
            if (m_DisableInSimulatorMode != null) {
                m_DisableInSimulatorMode.ForEach(go => {
                    if (go.activeSelf) {
                        go.SetActive(false);
                        m_DisabledGameObjects.Add(go);
                    }
                });
            }
            if (m_EnableInSimulatorMode != null) {
                m_EnableInSimulatorMode.ForEach(go => {
                    if (!go.activeSelf) {
                        go.SetActive(true);
                        m_EnabledGameObjects.Add(go);
                    }
                });
            }
        }

        private void OnDisable() {
            if (!AppTracker.IsPlaying) {
                return;
            }
            m_DisabledGameObjects.ForEach(go => go.SetActive(true));
            m_EnabledGameObjects.ForEach(go => go.SetActive(false));
        }
    }
}