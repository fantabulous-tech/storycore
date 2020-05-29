using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace RogoDigital.Lipsync.AutoSync
{
	[CustomEditor(typeof(ASPhonemeMarkerCleanupModule))]
	public class ASPhonemeMarkerCleanupModuleEditor : Editor
	{
		private ASPhonemeMarkerCleanupModule typedTarget;

		private void OnEnable()
		{
			typedTarget = (ASPhonemeMarkerCleanupModule)target;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (typedTarget.cleanupMode == ASPhonemeMarkerCleanupModule.CleanupMode.Legacy)
			{
				EditorGUILayout.HelpBox("Legacy mode is only intended to avoid breaking existing presets. We recommend switching to Simple or Advanced mode for much better results.", MessageType.Warning);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cleanupMode"));
				EditorGUILayout.Space();
				EditorGUILayout.Slider(serializedObject.FindProperty("cleanupAggression"), 0, 0.2f);
			}
			else if (typedTarget.cleanupMode == ASPhonemeMarkerCleanupModule.CleanupMode.Simple)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cleanupMode"));
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("allowRetiming"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("allowMerging"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Slider(serializedObject.FindProperty("maximumMarkerDensity"), 0, 1);
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cleanupMode"));
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("allowRetiming"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("allowMerging"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Slider(serializedObject.FindProperty("maximumGapForMerging"), 0, 0.2f);
				EditorGUILayout.Slider(serializedObject.FindProperty("maximumRetimingError"), 0, 0.2f);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}