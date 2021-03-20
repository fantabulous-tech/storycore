using Ink.Runtime;
using StoryCore;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Story Int", order = (int) MenuOrder.VariableString)]
    public class GameVariableStoryInt : GameVariableInt, IStoryVariable {
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

        protected override int GetValue() {
            object storyValue = Story?.variablesState[name];

            if (storyValue == null) {
                return base.GetValue();
            }

            if (storyValue is int intValue) {
                return intValue;
            }

            if (int.TryParse(storyValue.ToString(), out int parseIntValue)) {
                return parseIntValue;
            }

            Debug.LogWarning($"Couldn't get int value of '{name}': {storyValue}");
            return base.GetValue();
        }

        protected override void SetValue(int value) {
            base.SetValue(value);

            if (Story != null) {
                Story.variablesState[name] = value;
            } else {
                Debug.LogError($"Cannot set variable {name} because Story is unavailable.");
            }
        }

        public void SetInStory() {
            // Use the base current value, otherwise GetValue() will return the story's value, not the previous value.
            SetValue(m_CurrentValue);
        }

        public void Subscribe() {
            Story.ObserveVariable(name, OnVariableChanged);
        }

        public void Unsubscribe() {
            Story.RemoveVariableObserver(OnVariableChanged, name);
        }

        private void OnVariableChanged(string varName, object value) {
            ValueString = value.ToString();
            Raise();
        }
    }
}