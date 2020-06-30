using System.Collections.Generic;
using System.Linq;
using Bindings;
using StoryCore.AssetBuckets;
using StoryCore.GameVariables;
using UnityEngine;

namespace StoryCore {
    public class ScenarioList : MonoBehaviour {
        [SerializeField] private BaseBucket m_Bucket;
        [SerializeField] private RectTransform m_ContentLocation;
        [SerializeField] private ScenarioButton m_ButtonPrefab;
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;

        private void Start() {
            foreach (string itemName in m_Bucket.ItemNames.OrderBy(n => n)) {
                ScenarioButton scenarioButton = Instantiate(m_ButtonPrefab, m_ContentLocation, false);
                scenarioButton.SetText(itemName);
                scenarioButton.ScenarioPath = "scenario_" + itemName;
            }
        }
    }
}