using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using RogoDigital;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using System.Collections.Generic;
using System.IO;

public class AutoSyncWindow : ModalWindow
{
	private LipSyncClipSetup setup;

	// Single Mode
	private AudioClip clip;
	private int currentPreset = -1;
	private bool presetChanged = false;

	// Multiple Mode
	private List<AudioClip> clips;
	private List<Type> autoSyncModuleTypes;
	private Dictionary<Type, AutoSyncModuleInfoAttribute> moduleInfos;
	private int currentClip = 0;
	private bool xmlMode = false, loadTranscripts = true, batchIncomplete;

	private Vector2 batchScroll, presetScroll, settingsScroll;
	private int tab = 0;
	private List<AutoSyncModule> currentModules;
	private List<Editor> serializedModules;

	private Texture2D infoIcon, upIcon, downIcon, plusIcon, minusIcon, saveIcon;

	private AutoSync autoSyncInstance;
	private AutoSyncPreset[] presets;

	private void OnEnable()
	{
		currentModules = new List<AutoSyncModule>();
		serializedModules = new List<Editor>();

		clips = new List<AudioClip>();

		autoSyncModuleTypes = AutoSyncUtility.GetModuleTypes();
		presets = AutoSyncUtility.GetPresets();

		moduleInfos = new Dictionary<Type, AutoSyncModuleInfoAttribute>();
		for (int i = 0; i < autoSyncModuleTypes.Count; i++)
		{
			moduleInfos.Add(autoSyncModuleTypes[i], AutoSyncUtility.GetModuleInfo(autoSyncModuleTypes[i]));
		}

		infoIcon = EditorGUIUtility.FindTexture("console.infoicon.sml");

		if (EditorGUIUtility.isProSkin)
		{
			upIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/up.png");
			downIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/down.png");
			plusIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/plus.png");
			minusIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/minus.png");
			saveIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/save.png");
		}
		else
		{
			upIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/up.png");
			downIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/down.png");
			plusIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/plus.png");
			minusIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/minus.png");
			saveIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/save.png");
		}
	}

	void OnDestroy()
	{
		parent.currentModal = null;
		parent.Focus();

		if (presetChanged)
		{
			if (EditorUtility.DisplayDialog("Discard Preset Changes?", "You have made unsaved changes to the current preset. Are you sure you want to discard them?", "Yes", "No"))
			{
				for (int i = 0; i < currentModules.Count; i++)
				{
					DestroyImmediate(serializedModules[i]);
					DestroyImmediate(currentModules[i]);
				}
			}
			else
			{
				var window = CreateWindow(parent, setup, tab);
				window.OnEnable();
				window.currentModules = currentModules;
				window.currentPreset = currentPreset;
				window.presetChanged = presetChanged;
				window.serializedModules = serializedModules;
			}
		}
		else
		{
			for (int i = 0; i < currentModules.Count; i++)
			{
				DestroyImmediate(serializedModules[i]);
				DestroyImmediate(currentModules[i]);
			}
		}
	}

	void OnGUI()
	{
		EditorStyles.label.wordWrap = true;

		GUILayout.Space(10);
		tab = GUILayout.Toolbar(tab, new string[] { "AutoSync Settings", "Batch Process" });
		GUILayout.Space(10);

		bool ready = true;

		if (tab == 0)
		{
			ready = currentModules.Count > 0;

			GUILayout.Space(5);
			GUILayout.Box("Presets", EditorStyles.boldLabel);
			GUILayout.Space(5);
			presetScroll = GUILayout.BeginScrollView(presetScroll, GUILayout.MaxHeight(80));
			if (presets.Length == 0)
			{
				GUILayout.Space(10);
				GUILayout.Box("No Presets Found", EditorStyles.centeredGreyMiniLabel);
			}
			else
			{
				EditorGUILayout.BeginVertical();
				for (int i = -1; i < presets.Length; i++)
				{
					var lineRect = EditorGUILayout.BeginHorizontal();
					if (i == currentPreset)
					{
						GUI.Box(lineRect, "", (GUIStyle)"SelectionRect");
					}

					if (i >= 0)
					{
						if (i == currentPreset)
						{
							GUILayout.Button(presetChanged ? presets[i].displayName + " *" : presets[i].displayName, EditorStyles.label);
						}
						else
						{
							if (GUILayout.Button(presets[i].displayName, EditorStyles.label))
							{
								if (presetChanged)
								{
									if (EditorUtility.DisplayDialog("Discard Preset Changes?", "You have made unsaved changes to the current preset. Are you sure you want to discard them?", "Yes", "No"))
									{
										LoadPreset(i);
									}
								}
								else
								{
									LoadPreset(i);
								}
							}
						}
					}
					else
					{
						if (GUILayout.Button("None", EditorStyles.label))
						{
							if (presetChanged)
							{
								if (EditorUtility.DisplayDialog("Discard Preset Changes?", "You have made unsaved changes to the current preset. Are you sure you want to discard them?", "Yes", "No"))
								{
									LoadPreset(-1);
								}
							}
							else
							{
								LoadPreset(-1);
							}
						}
					}

					EditorGUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndScrollView();
			GUILayout.Space(5);
			var infoRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			GUI.Box(infoRect, "", EditorStyles.helpBox);
			GUILayout.Space(5);
			if (currentPreset == -1)
			{
				GUILayout.Box(new GUIContent("Select a preset for more information.", infoIcon), EditorStyles.label);
			}
			else
			{
				GUILayout.Box(presets[currentPreset].displayName, EditorStyles.boldLabel);
				EditorGUILayout.LabelField(presets[currentPreset].description, EditorStyles.label);
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(5);
			EditorGUILayout.EndVertical();
			GUILayout.Space(5);
			Rect toolbarRect = EditorGUILayout.BeginHorizontal();
			toolbarRect.x = 0;
			GUI.Box(toolbarRect, "", EditorStyles.toolbar);
			GUILayout.Box("Current Modules", EditorStyles.miniLabel);
			GUILayout.FlexibleSpace();
			Rect dropDownRect = EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Add Module", plusIcon, "Add a new module to the list"), EditorStyles.toolbarDropDown, GUILayout.Width(90)))
			{
				GenericMenu addMenu = new GenericMenu();
				for (int i = 0; i < autoSyncModuleTypes.Count; i++)
				{
					bool isAdded = false;

					for (int m = 0; m < currentModules.Count; m++)
					{
						if (currentModules[m].GetType() == autoSyncModuleTypes[i])
						{
							isAdded = true;
							break;
						}
					}

					if (isAdded)
					{
						addMenu.AddDisabledItem(new GUIContent(moduleInfos[autoSyncModuleTypes[i]].displayName));
					}
					else
					{
						int e = i;
						addMenu.AddItem(new GUIContent(moduleInfos[autoSyncModuleTypes[i]].displayName), false, () => { AddModule(e); });
					}
				}
				addMenu.AddSeparator("");
				addMenu.AddItem(new GUIContent("Get More Modules"), false, () => { RDExtensionWindow.ShowWindow("LipSync_Pro"); });
				addMenu.DropDown(dropDownRect);
			}
			GUILayout.Space(20);
			if (GUILayout.Button(new GUIContent("Save As New", saveIcon, "Save the current setup as a new preset"), EditorStyles.toolbarButton, GUILayout.Width(100)))
			{
				var savePath = EditorUtility.SaveFilePanelInProject("Save AutoSync Preset", "New AutoSync Preset", "asset", "");
				if (!string.IsNullOrEmpty(savePath))
				{
					AutoSyncPreset preset = null;

					if (File.Exists(savePath))
					{
						preset = AssetDatabase.LoadAssetAtPath<AutoSyncPreset>(savePath);
					}
					else
					{
						preset = CreateInstance<AutoSyncPreset>();
						preset.CreateFromModules(currentModules.ToArray());

						preset.displayName = Path.GetFileNameWithoutExtension(savePath);
						preset.description = "Using: ";
						for (int i = 0; i < currentModules.Count; i++)
						{
							preset.description += currentModules[i].GetType().Name;
							if (i < currentModules.Count - 1)
								preset.description += ", ";
						}
					}

					AssetDatabase.CreateAsset(preset, savePath);
					AssetDatabase.Refresh();

					presets = AutoSyncUtility.GetPresets();
					currentPreset = -1;
				}
			}

			EditorGUI.BeginDisabledGroup(currentPreset == -1 || !presetChanged);
			if (GUILayout.Button(new GUIContent("Save Changes", saveIcon, "Overwrite your changes to the current preset"), EditorStyles.toolbarButton, GUILayout.Width(100)))
			{
				if (EditorUtility.DisplayDialog("Overwrite Preset?", "Are you sure you want to overwrite the saved preset? This cannot be undone.", "Yes", "No"))
				{
					string path = AssetDatabase.GetAssetPath(presets[currentPreset]);

					if (!string.IsNullOrEmpty(path))
					{
						presets[currentPreset].CreateFromModules(currentModules.ToArray());
						AssetDatabase.SaveAssets();
						presetChanged = false;
					}
				}
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginVertical();
			settingsScroll = GUILayout.BeginScrollView(settingsScroll, false, false);
			if (currentModules.Count == 0)
			{
				GUILayout.Space(10);
				GUILayout.Box("No Modules Added", EditorStyles.centeredGreyMiniLabel);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				for (int i = 0; i < currentModules.Count; i++)
				{
					var type = currentModules[i].GetType();
					var info = moduleInfos[type];
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUILayout.Box(new GUIContent(info.displayName, infoIcon, info.description), EditorStyles.label);
					GUILayout.Space(10);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical(GUILayout.Width(15));
					GUILayout.Space(10);
					if (GUILayout.Button(new GUIContent(minusIcon, "Remove Module")))
					{
						DestroyImmediate(serializedModules[i]);
						DestroyImmediate(currentModules[i]);
						serializedModules.RemoveAt(i);
						currentModules.RemoveAt(i);
						break;
					}
					GUILayout.Space(5);
					EditorGUI.BeginDisabledGroup(i == 0);
					if (GUILayout.Button(new GUIContent(upIcon, "Move Up")))
					{
						currentModules.Insert(i - 1, currentModules[i]);
						currentModules.RemoveAt(i + 1);
						serializedModules.Insert(i - 1, serializedModules[i]);
						serializedModules.RemoveAt(i + 1);
						break;
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(i + 2 > currentModules.Count);
					if (GUILayout.Button(new GUIContent(downIcon, "Move Down")))
					{
						currentModules.Insert(i + 2, currentModules[i]);
						currentModules.RemoveAt(i);
						serializedModules.Insert(i + 2, serializedModules[i]);
						serializedModules.RemoveAt(i);
						break;
					}
					EditorGUI.EndDisabledGroup();
					GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					var missing = GetMissingClipFeaturesInClipEditor(currentModules, i);
					if (missing != ClipFeatures.None)
					{
						EditorGUILayout.HelpBox(string.Format("This module requires: {0}.\n These features must either be present in the clip already, or be provided by a module above this one.", missing), MessageType.Error);
					}
					serializedModules[i].OnInspectorGUI();
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
					GUILayout.Space(15);
				}
				if (EditorGUI.EndChangeCheck() && currentPreset > -1)
				{
					presetChanged = true;
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.Space(5);
			GUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(!ready);
			if (GUILayout.Button("Start Single Process", GUILayout.Height(25)))
			{
				if (autoSyncInstance == null)
					autoSyncInstance = new AutoSync();

				autoSyncInstance.RunSequence(currentModules.ToArray(), FinishedProcessingSingle, (LipSyncData)setup.data);
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
		}
		else
		{
			ready = clips.Count > 0 && currentModules.Count > 0;

			GUILayout.Space(5);
			GUILayout.Box("Select AudioClips", EditorStyles.boldLabel);
			GUILayout.Space(5);
			batchScroll = GUILayout.BeginScrollView(batchScroll);
			for (int a = 0; a < clips.Count; a++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(5);
				clips[a] = (AudioClip)EditorGUILayout.ObjectField(clips[a], typeof(AudioClip), false);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Remove", GUILayout.MaxWidth(200)))
				{
					clips.RemoveAt(a);
					break;
				}
				GUILayout.Space(5);
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(5);
			GUILayout.EndScrollView();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add AudioClip"))
			{
				clips.Add(null);
			}
			if (GUILayout.Button("Add Selected"))
			{
				foreach (AudioClip c in Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets))
				{
					if (!clips.Contains(c))
						clips.Add(c);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.HelpBox("Settings from the AutoSync Settings tab will be used. Make sure they are correct.", MessageType.Info);
			xmlMode = EditorGUILayout.Toggle("Export as XML", xmlMode);
			loadTranscripts = EditorGUILayout.Toggle("Load Transcripts .txt", loadTranscripts);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(!ready);
			if (GUILayout.Button("Start Batch Process", GUILayout.Height(25)))
			{
				currentClip = 0;

				if (clips.Count > 0)
				{
					if (autoSyncInstance == null)
						autoSyncInstance = new AutoSync();

					LipSyncData tempData = CreateInstance<LipSyncData>();
					tempData.clip = clips[currentClip];
					tempData.length = tempData.clip.length;

					if (loadTranscripts)
					{
						tempData.transcript = AutoSyncUtility.TryGetTranscript(tempData.clip);
					}

					autoSyncInstance.RunSequence(currentModules.ToArray(), FinishedProcessingMulti, tempData);
				}
				else
				{
					ShowNotification(new GUIContent("No clips added for batch processing!"));
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
		}
	}

	private void LoadPreset(int presetIndex)
	{
		for (int i = 0; i < currentModules.Count; i++)
		{
			DestroyImmediate(currentModules[i]);
			DestroyImmediate(serializedModules[i]);
		}
		currentModules.Clear();
		serializedModules.Clear();
		currentPreset = presetIndex;
		presetChanged = false;

		if (presetIndex >= 0)
		{
			for (int i = 0; i < presets[presetIndex].modules.Length; i++)
			{
				AddModule(presets[presetIndex].modules[i], presets[presetIndex].moduleSettings[i]);
			}
		}
	}

	private ClipFeatures GetMissingClipFeaturesInClipEditor(List<AutoSyncModule> modules, int index)
	{
		var module = modules[index];
		var req = module.GetCompatibilityRequirements();
		ClipFeatures metCriteria = 0;

		// Find which criteria are met, or will be met once the module chain has run as far as the provided index.

		for (int i = 0; i < index; i++)
		{
			metCriteria |= modules[i].GetOutputCompatibility();
		}

		if (clip)
			metCriteria |= ClipFeatures.AudioClip;

		if (!string.IsNullOrEmpty(setup.Transcript))
			metCriteria |= ClipFeatures.Transcript;

		if (setup.PhonemeData != null && setup.PhonemeData.Count > 0)
			metCriteria |= ClipFeatures.Phonemes;

		if (setup.EmotionData != null && setup.EmotionData.Count > 0)
			metCriteria |= ClipFeatures.Emotions;

		if (setup.GestureData != null && setup.GestureData.Count > 0)
			metCriteria |= ClipFeatures.Gestures;

		// Compare masks
		var inBoth = req & metCriteria;
		return inBoth ^ req;
	}

	private void AddModule(string name, string jsonData)
	{
		var module = (AutoSyncModule)CreateInstance(name);

		if (!module)
			return;

		if (!string.IsNullOrEmpty(jsonData))
		{
			JsonUtility.FromJsonOverwrite(jsonData, module);
		}

		module.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
		currentModules.Add(module);
		var editor = Editor.CreateEditor(module);
		serializedModules.Add(editor);
	}

	private void AddModule(int index)
	{
		var module = (AutoSyncModule)CreateInstance(autoSyncModuleTypes[index].Name);
		module.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
		currentModules.Add(module);
		var editor = Editor.CreateEditor(module);
		serializedModules.Add(editor);
	}

	private void FinishedProcessingMulti(LipSyncData outputData, AutoSync.ASProcessDelegateData data)
	{
		if (data.success)
		{
			var settings = LipSyncEditorExtensions.GetProjectFile();

			// Create File
			string outputPath = AssetDatabase.GetAssetPath(outputData.clip);
			outputPath = Path.ChangeExtension(outputPath, xmlMode ? "xml" : "asset");

			try
			{
				LipSyncClipSetup.SaveFile(settings, outputPath, xmlMode, outputData.transcript, outputData.length, outputData.phonemeData, outputData.emotionData,
					outputData.gestureData, outputData.clip);
			}
			catch (Exception e)
			{
				Debug.LogError(e.StackTrace);
			}
		}
		else
		{
			batchIncomplete = true;
			string clipName = "Undefined";
			if (outputData.clip)
			{
				clipName = outputData.clip.name;
			}

			Debug.LogErrorFormat("AutoSync: Processing failed on clip '{0}'. Continuing with batch.", clipName);
		}

		if (currentClip < clips.Count - 1)
		{
			currentClip++;

			autoSyncInstance = new AutoSync();

			LipSyncData tempData = CreateInstance<LipSyncData>();
			tempData.clip = clips[currentClip];
			tempData.length = tempData.clip.length;

			if (loadTranscripts)
			{
				tempData.transcript = AutoSyncUtility.TryGetTranscript(tempData.clip);
			}

			autoSyncInstance.RunSequence(currentModules.ToArray(), FinishedProcessingMulti, tempData);
		}
		else
		{
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();

			if (!batchIncomplete)
			{
				setup.ShowNotification(new GUIContent("Batch AutoSync Completed Successfully"));
			}
			else
			{
				setup.ShowNotification(new GUIContent("Batch AutoSync Completed With Errors"));
			}

			Close();
		}
	}

	private void FinishedProcessingSingle(LipSyncData outputData, AutoSync.ASProcessDelegateData data)
	{
		if (data.success)
		{
			setup.data = (TemporaryLipSyncData)outputData;
			setup.changed = true;
			setup.previewOutOfDate = true;
			setup.disabled = false;
			setup.ShowNotification(new GUIContent(string.Format("AutoSync Completed {0}{1}", string.IsNullOrEmpty(data.message) ? "" : ":", data.message)));
			Close();
		}
		else
		{
			Debug.LogFormat("AutoSync Failed: {0}", data.message);
			ShowNotification(new GUIContent(data.message));
		}
	}

	public static AutoSyncWindow CreateWindow(ModalParent parent, LipSyncClipSetup setup, int mode, params int[] modules)
	{
		AutoSyncWindow window = CreateInstance<AutoSyncWindow>();

		window.position = new Rect(parent.center.x - 250, parent.center.y - 400, 500, 800);
		window.minSize = new Vector2(400, 500);
		window.titleContent = new GUIContent("AutoSync");

		window.setup = setup;

		window.tab = mode;
		window.clip = setup.Clip;

		window.OnEnable();
		for (int i = 0; i < modules.Length; i++)
		{
			window.AddModule(modules[i]);
		}

		window.Show(parent);
		return window;
	}

	[MenuItem("Window/Rogo Digital/LipSync Pro/Batch Process", false, 12)]
	private static void OpenFromMenu()
	{
		var clipSetup = LipSyncClipSetup.ShowWindow();
		CreateWindow(clipSetup, clipSetup, 1);
	}
}