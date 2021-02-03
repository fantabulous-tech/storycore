using CoreUtils;
using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Choices {
    [CustomEditor(typeof(DistractedTracker))]
    public class DistractedChoiceEditor : Editor<DistractedTracker> {
        private void OnEnable() {
            if (Target != null) {
                Target.AttentionChanged += Repaint;
            }
        }

        private void OnDisable() {
            if (Target != null) {
                Target.AttentionChanged -= Repaint;
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (AppTracker.IsPlaying) {
                GUI.enabled = false;
                EditorGUILayout.Toggle("IsPayingAttention", Target.IsPayingAttention);
            }
        }
    }
}