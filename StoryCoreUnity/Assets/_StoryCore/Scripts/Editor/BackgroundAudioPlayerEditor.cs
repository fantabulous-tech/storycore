using StoryCore.Audio;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Editor {
    [CustomEditor(typeof(LoopingAudioPlayer))]
    public class BackgroundAudioPlayerEditor : Editor<LoopingAudioPlayer> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Test Goddess")) {
                Target.Play("goddess");
            }
            if (GUILayout.Button("Test Entry")) {
                Target.Play("entry");
            }
            if (GUILayout.Button("Test Headmistress")) {
                Target.Play("headmistress");
            }
        }
    }
}