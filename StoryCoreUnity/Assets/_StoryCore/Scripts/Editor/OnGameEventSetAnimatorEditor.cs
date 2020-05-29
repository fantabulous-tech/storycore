using System;
using StoryCore.Utils;
using UnityEditor;

namespace StoryCore.GameEvents {
    [CustomEditor(typeof(OnGameEventSetAnimator))]
    public class OnGameEventSetAnimatorEditor : Editor<OnGameEventSetAnimator> {
        public override void OnInspectorGUI() {
            switch (Target.Type) {
                case OnGameEventSetAnimator.ParamType.Bool:
                    DrawPropertiesExcluding(serializedObject, "m_IntValue", "m_FloatValue");
                    break;
                case OnGameEventSetAnimator.ParamType.Int:
                    DrawPropertiesExcluding(serializedObject, "m_BoolValue", "m_FloatValue");
                    break;
                case OnGameEventSetAnimator.ParamType.Float:
                    DrawPropertiesExcluding(serializedObject, "m_BoolValue", "m_IntValue");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}