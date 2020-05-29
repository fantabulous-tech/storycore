using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using StoryCore.Utils;

namespace RogoDigital.Lipsync.AutoSync
{
	[CustomEditor(typeof(ASMontrealPhonemeDetectionModule))]
	public class ASMontrealPhonemeDetectionModuleEditor : Editor
	{
		private string[] languageModelNames;
		private Object conversionTable;

		private void OnEnable ()
		{
			languageModelNames = ASMontrealLanguageModel.FindModels();
			
			ASMontrealPhonemeDetectionModule module = (ASMontrealPhonemeDetectionModule) target;
			if (!module.conversionTablePath.IsNullOrEmpty()) {
				conversionTable = AssetDatabase.LoadAssetAtPath<Object>(module.conversionTablePath);
			}
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update();

			var lmProp = serializedObject.FindProperty("languageModel");
			lmProp.intValue = EditorGUILayout.Popup(lmProp.displayName, lmProp.intValue, languageModelNames);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useAudioConversion"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minLengthForSustain"));
			EditorGUILayout.Space();
			var retry = serializedObject.FindProperty("autoRetry");
			EditorGUILayout.PropertyField(retry);
			if (retry.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAttempts"));
			}

			Object newConversionTable = EditorGUILayout.ObjectField("Conversion Table (.tsv)", conversionTable, typeof(Object), false);

			if (newConversionTable && conversionTable != newConversionTable) {
				conversionTable = newConversionTable;
				string path = AssetDatabase.GetAssetPath(conversionTable);
				if (!path.EndsWith(".tsv")) {
					conversionTable = null;
					Debug.LogWarning("Only add .tsv files.");
				} else {
					ASMontrealPhonemeDetectionModule module = (ASMontrealPhonemeDetectionModule) target;
					module.conversionTablePath = AssetDatabase.GetAssetPath(conversionTable);
					EditorUtility.SetDirty(module);
				}
			}

			//EditorGUILayout.PropertyField(serializedObject.FindProperty("configPath"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}