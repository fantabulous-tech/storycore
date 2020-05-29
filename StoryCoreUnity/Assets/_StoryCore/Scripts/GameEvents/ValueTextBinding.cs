using StoryCore.GameVariables;
using TMPro;
using UnityEngine;

namespace Bindings {
    public class ValueTextBinding : MonoBehaviour {
        [SerializeField] private BaseGameVariable m_RangeVariable;
        [SerializeField] private TextMeshProUGUI m_Label;

        private void Start() {
            m_RangeVariable.GenericEvent += OnVariableChanged;
            m_Label.text = m_RangeVariable.ValueString;
        }

        private void OnVariableChanged() {
            m_Label.text = m_RangeVariable.ValueString;
        }

        private void OnValidate() {
            if (m_Label && m_RangeVariable) {
                m_Label.text = m_RangeVariable.ValueString;
            }
        }
    }
}