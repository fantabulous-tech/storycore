using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Story String", order = (int) MenuOrder.VariableString)]
    public class GameVariableStoryString : BaseGameVariableStory<GameVariableStoryString, string> {
        // protected override void Init() {
        // 	base.Init();
        //
        // 	if (Globals.IsActive) { Globals.StoryTeller.Story.ObserveVariable(name, OnVariableChanged); }
        // }

        // private void OnVariableChanged(string varName, object value) { Raise(); }

        protected override string Parse(string stringValue) {
            return stringValue;
        }

        protected override string GetValue() {
            if (m_Story == null) {
                GetStory();
            }

            return m_Story == null ? m_InitialValue : Parse(m_Story.variablesState[name].ToString());
        }
    }
}