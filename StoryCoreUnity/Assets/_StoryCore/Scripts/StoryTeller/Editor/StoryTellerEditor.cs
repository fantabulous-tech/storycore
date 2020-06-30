using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using EditorStyles = UnityEditor.EditorStyles;

namespace StoryCore {
    [CustomEditor(typeof(StoryTeller))]
    public class StoryTellerEditor : Editor<StoryTeller> {
        private GUIStyle m_RedLabel;

        private GUIStyle RedLabel => m_RedLabel ?? (m_RedLabel = new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = Color.red}});

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

            EditorGUILayout.LabelField("Current Text", Target.Story.currentText);

            for (int i = 0; i < Target.CurrentChoices.Count; i++) {
                StoryChoice choice = Target.CurrentChoices[i];
                if (GUILayout.Button(choice.Text)) {
                    choice.Choose();
                }
            }
        }
    }
}