using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using StoryCore.AssetBuckets;
using StoryCore.InkTodo;
using StoryCore.Utils;
using StoryCore.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace StoryCore {
    public static class LipSyncAudioChecker {
        private const string kVoiceScriptPath = "Voice Script.txt";
        private const string kMissingLinesPath = "Missing Lines Script.txt";
        private static Queue<string> s_LipSyncQueue;
        private static ProgressBarCounted s_AutoSyncProgress;
        private static int s_AutoSyncProgressTotal;
        private static int s_LoopCounter;
        private static AutoSync s_AutoSyncInstance = new AutoSync();
        private static VOBucket s_VOBucket;
        private static bool s_WasManual;

        private static VOBucket VOBucket => UnityUtils.GetOrSet(ref s_VOBucket, () => AssetDatabase.LoadAssetAtPath<VOBucket>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:VOBucket").First())));

        // [MenuItem("Window/Rogo Digital/Run LipSync on VO Missing Transcript")]
        // public static void RunAutoSyncOnVO() {
        // 	string[] paths = AssetDatabase.FindAssets("t:LipSyncData")
        // 		.Select(AssetDatabase.GUIDToAssetPath)
        // 		.Where(NeedsLipSyncPass)
        // 		.ToArray();
        //
        // 	if (!EditorUtility.DisplayDialog("Run Auto-Sync on ALL VO files.", $"Are you sure you want to auto-sync {paths.Length} files?", "Yes", "Cancel")) {
        // 		return;
        // 	}
        //
        // 	StartAutoSync(paths);
        // }

        [MenuItem("Window/Rogo Digital/Check VO Lip Sync Files")]
        public static void CheckLipSyncAudio() {
            // Check for errors in LipSyncData files.
            AssetDatabase.FindAssets("t:LipSyncData").ForEach(CheckLipSyncData);

            // Check for missing LipSyncData files.
            AssetDatabase.FindAssets("t:AudioClip").ForEach(CheckAudioClip);

            Debug.Log("Check complete.");
        }

        [MenuItem("Window/Rogo Digital/Create Missing Lip Sync Files")]
        private static void CreateMissingLipSyncData() {
            InkUtils.RefreshTranscript();

            s_WasManual = VOBucket.ManualUpdate;
            VOBucket.ManualUpdate = true;

            string[] paths = AssetDatabase.FindAssets("t:AudioClip")
                                          .Select(AssetDatabase.GUIDToAssetPath)
                                          .Where(HasMissingData)
                                          .ToArray();

            int count = paths.Length;

            if (count == 0) {
                Debug.Log("No missing lip sync files found.");
                return;
            }

            const int kMaxCount = 30;

            string missing = count > kMaxCount + 10 ? paths.Take(kMaxCount).AggregateToString("\n") : paths.AggregateToString("\n");

            if (count > kMaxCount + 10) {
                missing += $"\n...\n+{count - kMaxCount} more.";
            }

            if (EditorUtility.DisplayDialog("Mission Lip Sync Data Found", $"Create new Lip Sync Data for these files?\n\n{missing}", "Create", "Cancel")) {
                StartAutoSync(paths);
            } else {
                Debug.Log($"Lip sync data creation skipped for {count} files.");
            }
        }

        [MenuItem("Window/Rogo Digital/Create Missing Lip Sync Files x1")]
        private static void CreateNextMissingLipSyncData() {
            InkUtils.RefreshTranscript();

            string[] paths = AssetDatabase.FindAssets("t:AudioClip")
                                          .Select(AssetDatabase.GUIDToAssetPath)
                                          .Where(HasMissingData)
                                          .ToArray();

            if (paths.Length == 0) {
                Debug.Log("No missing lip sync files found.");
                return;
            }

            paths = new[] {paths.First()};
            StartAutoSync(paths);
        }

        [MenuItem("Window/Rogo Digital/Export Transcript")]
        public static void ExportScript() {
            InkUtils.RefreshTranscript();
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in InkUtils.TranscriptLookup) {
                sb.AppendLine($"{kvp.Key}\t{kvp.Value}");
            }
            FileUtils.WriteAllText(kVoiceScriptPath, sb.ToString());
            System.Diagnostics.Process.Start(kVoiceScriptPath);
        }

        [MenuItem("Window/Rogo Digital/Export Missing Lines")]
        public static void ExportMissingLines() {
            StringBuilder sb = new StringBuilder();

            InkUtils.RefreshTranscript();

            foreach (KeyValuePair<string, string> kvp in InkUtils.TranscriptLookup) {
                if (HasVO(kvp.Key)) {
                    continue;
                }

                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            FileUtils.WriteAllText(kMissingLinesPath, sb.ToString());
            System.Diagnostics.Process.Start(kMissingLinesPath);
        }

        private static bool HasVO(string key) {
            return VOBucket.Has(key);
        }

        private static void CheckAudioClip(string guid) {
            string clipPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!clipPath.Contains("_VO")) {
                return;
            }

            string clipTestPath = Path.ChangeExtension(clipPath, ".asset");

            if (clipTestPath.IsNullOrEmpty() || File.Exists(clipTestPath)) {
                return;
            }

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

            Debug.LogWarning($"No lip sync data found for '{clipPath}'", clip);
        }

        private static bool NeedsLipSyncPass(string path) {
            if (path == null || !path.Contains("_VO")) {
                return false;
            }

            LipSyncData lsd = AssetDatabase.LoadAssetAtPath<LipSyncData>(path);
            string scriptTranscript = InkUtils.GetTranscript(path, lsd);

            if (scriptTranscript.IsNullOrEmpty()) {
                return false;
            }

            return !lsd || lsd.transcript.IsNullOrEmpty();
        }

        private static bool HasMissingData(string path) {
            if (!path.Contains("_VO")) {
                return false;
            }

            string lipSyncPath = Path.ChangeExtension(path, ".asset");
            return !lipSyncPath.IsNullOrEmpty() && !File.Exists(lipSyncPath);
        }

        private static void CheckLipSyncData(string guid) {
            string filePath = AssetDatabase.GUIDToAssetPath(guid);

            if (!filePath.Contains("_VO")) {
                return;
            }

            // Try to get lipSync data.
            LipSyncData lipSync = AssetDatabase.LoadAssetAtPath<LipSyncData>(filePath);
            string lipSyncPath;
            string clipPath;

            if (lipSync != null) {
                lipSyncPath = filePath;
                clipPath = AssetDatabase.GetAssetPath(lipSync.clip);
            } else {
                // Path is not a lip sync data file, so try to find the audio clip.
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(filePath);

                if (!clip) {
                    Debug.LogError("Could not find lip sync data or audio clip here: " + filePath);
                    return;
                }

                clipPath = filePath;
                lipSyncPath = Path.ChangeExtension(clipPath, ".asset");
                lipSync = AssetDatabase.LoadAssetAtPath<LipSyncData>(lipSyncPath);
            }

            string transcript = InkUtils.GetTranscript(lipSyncPath, lipSync);
            if (lipSync.transcript.IsNullOrEmpty() && !transcript.IsNullOrEmpty()) {
                Debug.Log($"Transcript missing on {InkUtils.GetFolderName(filePath)}.{lipSync.name}: {transcript}", lipSync);
            }

            if (!lipSync.clip) {
                Debug.LogWarning($"Lip sync {lipSync.name} doesn't have a clip assigned.", lipSync);
            }

            string clipTestPath = Path.ChangeExtension(clipPath, ".asset");

            if (!lipSyncPath.IsNullOrEmpty() && !clipTestPath.IsNullOrEmpty() && !lipSyncPath.Equals(clipTestPath, StringComparison.OrdinalIgnoreCase)) {
                Debug.LogWarning($"Lip sync file '{lipSyncPath}' doesn't match clip '{clipPath}'", lipSync);
            }
        }

        private static void StartAutoSync(IReadOnlyCollection<string> paths) {
            s_AutoSyncProgressTotal = paths.Count;
            s_AutoSyncProgress = new ProgressBarCounted("Running Auto-Sync", s_AutoSyncProgressTotal, true);
            s_LipSyncQueue = new Queue<string>(paths);
            s_LoopCounter = 0;
            InkUtils.RefreshTranscript();
            RunNextAutoSync();
        }

        private static void RunNextAutoSync() {
            try {
                while (s_LipSyncQueue.Count > 0) {
                    string path = s_LipSyncQueue.Dequeue();

                    s_AutoSyncProgress.StartStep(s_AutoSyncProgressTotal - s_LipSyncQueue.Count, path);
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

                    if (clip == null) {
                        LipSyncData lipSync = AssetDatabase.LoadAssetAtPath<LipSyncData>(path);

                        if (lipSync == null) {
                            Debug.LogError($"No auto-syncable file found at {path}");
                            continue;
                        }

                        clip = lipSync.clip;
                    }

                    LipSyncData tempData = ScriptableObject.CreateInstance<LipSyncData>();
                    tempData.clip = clip;
                    tempData.length = tempData.clip.length;
                    tempData.transcript = CleanTranscript(InkUtils.GetTranscript(path, clip));

                    if (tempData.transcript.IsNullOrEmpty()) {
                        continue;
                    }

                    s_LoopCounter++;
                    int step = s_AutoSyncProgressTotal - s_LipSyncQueue.Count;
                    string info = $"AutoSync ({step}/{s_AutoSyncProgressTotal}): Processing {InkUtils.GetFolderName(path)}.{clip.name} to {tempData.transcript}";
                    Debug.Log(info, clip);
                    s_AutoSyncProgress.StartStep(step, info);
                    s_AutoSyncInstance.RunSequence(InkUtils.AutoSyncModules, FinishedAutoSync, tempData, true);
                    break;
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                s_AutoSyncProgress.Done();
            }

            if (s_LipSyncQueue.Count == 0) {
                s_AutoSyncProgress.Done();
            }
        }

        private static string CleanTranscript(string transcript) {
            if (transcript.Contains('<')) {
                transcript = transcript.Replace(@"<br/>", " ");
                transcript = transcript.ReplaceRegex(@"<[^>]*>", "");
            }

            return transcript;
        }

        private static void FinishedAutoSync(LipSyncData outputData, AutoSync.ASProcessDelegateData data) {
            if (data.success) {
                LipSyncProject settings = LipSyncEditorExtensions.GetProjectFile();

                // Create File
                string outputPath = AssetDatabase.GetAssetPath(outputData.clip);
                outputPath = Path.ChangeExtension(outputPath, "asset");

                // Add rest marker to the end. (Unnecessary?)
                // PhonemeMarker restMarker = new PhonemeMarker(InkUtils.RestIndex, 1);
                // outputData.phonemeData = outputData.phonemeData.Append(restMarker).ToArray();

                try {
                    LipSyncClipSetup.SaveFile(settings, outputPath, false, outputData.transcript, outputData.length,
                                              outputData.phonemeData, outputData.emotionData, outputData.gestureData, outputData.clip);
                }
                catch (Exception e) {
                    Debug.LogError(e.StackTrace);
                }
            } else {
                // batchIncomplete = true;
                AudioClip clip = outputData == null ? null : outputData.clip;
                string clipName = clip != null ? clip.name : "Undefined";
                Debug.LogError($"AutoSync: Processing failed on clip '{clipName}' because {data.message} Continuing with batch.", clip);
            }

            if (s_LipSyncQueue.Count > 0) {
                EditorApplication.delayCall += RunNextAutoSync;
            } else {
                Debug.Log($"AutoSync: Finished creating {s_LoopCounter} AutoSync files.");
                s_AutoSyncProgress.Done();
                VOBucket.ManualUpdate = s_WasManual;
                AssetBucketWatcher.FindReferences(VOBucket);
            }
        }
    }
}