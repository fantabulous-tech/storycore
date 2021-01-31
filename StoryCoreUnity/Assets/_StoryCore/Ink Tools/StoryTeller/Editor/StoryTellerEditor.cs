using CoreUtils.Editor;
using CoreUtils;
using UnityEditor;
using UnityEngine;
using EditorStyles = UnityEditor.EditorStyles;

namespace StoryCore {
    [CustomEditor(typeof(StoryTeller))]
    public class StoryTellerEditor : Editor<StoryTeller> {
        private GUIStyle m_RedLabel;

        private GUIStyle RedLabel => m_RedLabel ?? (m_RedLabel = new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = Color.red}});

        private bool m_ChoicesOpen = true;
        private bool m_VariablesOpen;

        private void OnEnable() {
            if (Target != null) {
                Target.OnChoicesReady += Repaint;
                Target.OnChosen += Repaint;
                Target.OnNext += Repaint;
            }
        }

        private void OnDisable() {
            if (Target != null) {
                Target.OnChoicesReady -= Repaint;
                Target.OnChosen -= Repaint;
                Target.OnNext -= Repaint;
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (!Application.isPlaying || Target.Story == null) {
                return;
            }

            if (Target.Story.currentErrors != null) {
                EditorGUILayout.LabelField("Current Text", Target.Story.currentErrors.AggregateToString("\n"), RedLabel);
            }

            EditorGUILayout.LabelField("Current Text", Target.Story.currentText.TrimEnd('\n'));

            m_ChoicesOpen = EditorGUILayout.Foldout(m_ChoicesOpen, "Choices");
            if (m_ChoicesOpen) {
                for (int i = 0; i < Target.CurrentChoices.Count; i++) {
                    StoryChoice choice = Target.CurrentChoices[i];
                    if (GUILayout.Button($"{i + 1}: {choice.Key}")) {
                        choice.Choose();
                    }
                }
            }
            
            m_VariablesOpen = EditorGUILayout.Foldout(m_VariablesOpen, "Variables");
            if (m_VariablesOpen) {
                foreach (var variable in Target.Story.variablesState) {
                    EditorGUILayout.LabelField(variable, Target.Story.variablesState[variable].ToString());
                }
            }

        }
    }
}