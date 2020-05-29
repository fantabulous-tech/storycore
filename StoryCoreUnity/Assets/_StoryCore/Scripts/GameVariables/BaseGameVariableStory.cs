using StoryCore.GameEvents;
using Ink.Runtime;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.GameVariables {
    public interface IStoryVariable {
        void SetInStory();
        void Subscribe();
        void Unsubscribe();
    }

    public abstract class BaseGameVariableStory<thisT, T> : BaseGameVariable<thisT, T>, IStoryVariable where thisT : BaseGameVariableStory<thisT, T> {
        [SerializeField, AutoFillAsset] private StoryTellerLocator m_StoryTellerLocator;

        private StoryTeller StoryTeller => m_StoryTellerLocator ? m_StoryTellerLocator.Value : null;

        protected Story m_Story;

        protected override void Init() {
            base.Init();
            if (m_StoryTellerLocator != null) {
                m_StoryTellerLocator.Changed += StoryTellerChanged;
            }
            GetStory();
        }

        protected void GetStory() {
            if (m_StoryTellerLocator == null || !m_StoryTellerLocator.Value) {
                return;
            }

            m_Story = m_StoryTellerLocator.Value.Story;

            if (m_Story == null) {
                StoryTeller.OnStoryReady -= GetStory;
                StoryTeller.OnStoryReady += GetStory;
                return;
            }

            StoryTeller.OnStoryReady -= GetStory;
            Unsubscribe();
            Subscribe();
            Raise();
        }

        public void Subscribe() {
            m_Story.ObserveVariable(name, OnVariableChanged);
        }

        public void Unsubscribe() {
            m_Story.RemoveVariableObserver(OnVariableChanged, name);
        }

        private void StoryTellerChanged(StoryTeller obj) {
            GetStory();
        }

        private void OnVariableChanged(string varName, object value) {
            ValueString = value.ToString();
            Raise();
        }

        public void SetInStory() {
            // Use the base current value, otherwise GetValue() will return the story's value, not the previous value.
            SetValue(m_CurrentValue);
        }

        protected override void SetValue(T value) {
            base.SetValue(value);

            if (m_Story == null) {
                GetStory();
            }

            if (m_Story != null) {
                m_Story.variablesState[name] = value;
            } else {
                Debug.LogError($"Cannot set variable {name} because there is not Story available.");
            }
        }
    }
}