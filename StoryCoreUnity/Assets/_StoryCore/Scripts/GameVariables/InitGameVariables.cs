using StoryCore.AssetBuckets;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;

public class InitGameVariables : MonoBehaviour {
    [SerializeField] private GameVariableBucket m_GameVariableBucket;

    private void Awake() {
        foreach (BaseGameVariable gameVariable in m_GameVariableBucket.Items) {
            gameVariable.TryInit();
        }
    }
}