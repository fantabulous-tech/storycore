using Ink.Runtime;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Story Bool", order = (int) MenuOrder.VariableString)]
    public class GameVariableStoryBool : GameVariableBool, IStoryVariable {
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        private Story m_Story;

        private StoryTeller StoryTeller => m_StoryTellerLocator ? m_StoryTellerLocator.Value : null;
        private Story Story => UnityUtils.GetOrSet(ref m_Story, GetStory);

        protected override void Init() {
            base.Init();
            if (m_StoryTellerLocator != null) {
                m_StoryTellerLocator.Changed += StoryTellerChanged;
            }
            UpdateStory();
        }

        private void StoryTellerChanged(StoryTeller obj) {
            UpdateStory();
        }

        private void UpdateStory() {
            if (!StoryTeller) {
                return;
            }
            
            StoryTeller.OnStoryReady -= UpdateStory;

            if (!Story) {
                // Wait until story is ready.
                StoryTeller.OnStoryReady += UpdateStory;
            } else {
                // Subscribe to changes in story and raise so game matches story.
                Unsubscribe();
                Subscribe();
                Raise();
            }
        }

        private Story GetStory() {
            return StoryTeller ? StoryTeller.Story : null;
        }

        protected override bool Parse(string stringValue) {
            return int.TryParse(stringValue, out int parseIntValue) ? parseIntValue > 0 : base.Parse(stringValue);
        }

        protected override bool GetValue() {
            if (Story == null) {
                return base.GetValue();
            }

            object storyValue = Story.variablesState[Name];

            switch (storyValue) {
                case int intValue:
                    return intValue > 0;
                case bool boolValue:
                    return boolValue;
                default:
                    return Parse(storyValue.ToString());
            }
        }

        protected override void SetValue(bool value) {
            base.SetValue(value);

            if (Story != null) {
                Story.variablesState[Name] = value;
            } else {
                Debug.LogError($"Cannot set variable {Name} because Story is unavailable.");
            }
        }

        public void SetInStory() {
            // Use the base current value, otherwise GetValue() will return the story's value, not the previous value.
            SetValue(m_CurrentValue);
        }

        public void Subscribe() {
            Story.ObserveVariable(Name, OnVariableChanged);
        }

        public void Unsubscribe() {
            Story.RemoveVariableObserver(OnVariableChanged, Name);
        }

        private void OnVariableChanged(string varName, object value) {
            ValueString = value.ToString();
            Raise();
        }
    }
}