using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CoreUtils;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using CoreUtils.AssetBuckets;
using CoreUtils.Editor;
using StoryCore.InkTodo;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore {
    public static class LipSyncAudioChecker {
        private const string kVoiceScriptPath = "Exports\\Voice Script.txt";
        private const string kFormattedVoiceScriptPath = "Exports\\Voice Script.html";
        private const string kFormattedMissingLinesPath = "Exports\\Missing Lines Script.html";
        private const string kMissingLinesPath = "Exports\\Missing Lines Script.txt";
        private static Queue<string> s_LipSyncQueue;
        private static ProgressBarCounted s_AutoSyncProgress;
        private static int s_AutoSyncProgressTotal;
        private static int s_LoopCounter;
        private static AutoSync s_AutoSyncInstance = new AutoSync();
        private static VOBucket s_VOBucket;
        private static bool s_WasManual;

        private static VOBucket VOBucket => UnityUtils.GetOrSet(ref s_VOBucket, () => AssetDatabase.LoadAssetAtPath<VOBucket>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:VOBucket").First())));

        [MenuItem("Window/VO and Ink Tools/Check VO Lip Sync Files", false, (int)MenuOrder.VOTools)]
        public static void CheckLipSyncAudio() {
            InkUtils.RefreshTranscript();

            // Check for errors in LipSyncData files.
            AssetDatabase.FindAssets("t:LipSyncData").ForEach(CheckLipSyncData);

            // Check for missing LipSyncData files.
            AssetDatabase.FindAssets("t:AudioClip").ForEach(CheckAudioClip);

            Debug.Log("Check complete.");
        }

        [MenuItem("Window/VO and Ink Tools/Create Missing Lip Sync Files", false, (int)MenuOrder.VOTools)]
        public static void CreateMissingLipSyncData() {
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

        [MenuItem("Window/VO and Ink Tools/Create Missing Lip Sync Files x1", false, (int)MenuOrder.VOTools)]
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

        [MenuItem("Window/VO and Ink Tools/Export Transcript", false, (int)MenuOrder.VOTools)]
        public static void ExportScript() {
            FileUtils.WriteAllText(kVoiceScriptPath, GetScript());
            System.Diagnostics.Process.Start(kVoiceScriptPath);
        }

        [MenuItem("Window/VO and Ink Tools/Export Transcript to HTML", false, (int)MenuOrder.VOTools)]
        public static void ExportFormattedScript() {
            string formattedScript = GetFormattedScript();
            FileUtils.WriteAllText(kFormattedVoiceScriptPath, formattedScript);
            System.Diagnostics.Process.Start(kFormattedVoiceScriptPath);
        }

        [MenuItem("Window/VO and Ink Tools/Export Missing Lines", false, (int)MenuOrder.VOTools)]
        public static void ExportMissingLines() {
            FileUtils.WriteAllText(kMissingLinesPath, GetMissingLines());
            System.Diagnostics.Process.Start(kMissingLinesPath);
        }
        
        [MenuItem("Window/VO and Ink Tools/Export Missing Lines to HTML", false, (int)MenuOrder.VOTools)]
        public static void ExportFormattedMissingLines() {
            string formattedScript = GetFormattedScript(true);
            FileUtils.WriteAllText(kFormattedMissingLinesPath, formattedScript);
            System.Diagnostics.Process.Start(kFormattedMissingLinesPath);
        }

        [MenuItem("Assets/Export Script to HTML", true)]
        [MenuItem("Assets/Export Missing Lines to HTML", true)]
        public static bool ValidateExportFromInkSelection() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return Selection.activeObject is DefaultAsset && File.Exists(path);
        }

        [MenuItem("Assets/Export Script to HTML", false, 72)]
        public static void ExportFullScriptToHtmlFromSelection() {
            ExportFormattedScriptFromSelection(false);
        }

        [MenuItem("Assets/Export Missing Lines to HTML", false, 73)]
        public static void ExportMissingLinesToHtmlFromSelection() {
            ExportFormattedScriptFromSelection(true);
        }

        private static void ExportFormattedScriptFromSelection(bool missingLinesOnly) {
            string rootPath = Path.GetDirectoryName(InkUtils.kStoryScriptPath)?.Replace('\\', '/').TrimEnd('/');

            foreach (Object obj in Selection.objects) {
                if (!obj) {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                string ext = Path.GetExtension(path);

                if (!ext.Equals(".ink", StringComparison.OrdinalIgnoreCase) && !ext.IsNullOrEmpty()) {
                    continue;
                }
                
                InkFileInfo info = new InkFileInfo(path, rootPath);
                string formattedScript = GetFormattedScriptFromInkFile(info, missingLinesOnly);
                string exportPath = missingLinesOnly ? kFormattedMissingLinesPath : kFormattedVoiceScriptPath;
                string fileName = $"{Path.GetFileNameWithoutExtension(exportPath)} {Path.GetFileName(Path.GetDirectoryName(path))}_{Path.GetFileNameWithoutExtension(path)}.html";
                exportPath = Path.Combine(Path.GetDirectoryName(exportPath) ?? "", fileName);
                FileUtils.WriteAllText(exportPath, formattedScript);
                System.Diagnostics.Process.Start(exportPath);
            }
        }

        private static string GetFormattedScript(bool missingLinesOnly = false) {
            StringBuilder sb = new StringBuilder();
            List<InkFileInfo> inkFiles = InkUtils.GetInkFiles(InkUtils.kStoryScriptPath);

            foreach (InkFileInfo info in inkFiles) {
                BuildFormattedScriptFromInkFile(info, sb, missingLinesOnly);
            }

            return WrapFormattedScript(sb.ToString());
        }

        private static string GetFormattedScriptFromInkFile(InkFileInfo info, bool missingLinesOnly = false) {
            if (info.Lines.Any(InkEditorUtils.CanTag)) {
                if (EditorUtility.DisplayDialog("Untagged Lines Found", "There are untagged lines in this file. Do you want to tag them first?", "Yes", "Skip")) {
                    InkEditorUtils.TagInkLinesAtPath(info.Path, info.InkAsset);
                    info.Refresh();
                }
            }

            StringBuilder sb = new StringBuilder();
            BuildFormattedScriptFromInkFile(info, sb, missingLinesOnly);
            return WrapFormattedScript(sb.ToString());
        }

        private static string WrapFormattedScript(string bodyHtml) {
            bodyHtml = bodyHtml.IsNullOrEmpty() ? "<h1>No lines found.</h1>" : bodyHtml;
            return $@"<html>
<head>
<style>
p {{text-align: center; font-family:'Courier New', Courier, monospace;}}
span {{color: #D3D3D3; font-size: 16; font-family:'Courier New', Courier, monospace; font-weight: normal;}}
</style>
</head>
<body>
{bodyHtml}</body>
</html>";
        }

        private static void BuildFormattedScriptFromInkFile(InkFileInfo info, StringBuilder sb, bool missingLinesOnly) {
            string currentKnot = null;
            string newKnot = null;
            string currentStitch = null;
            string newStitch = null;
            string currentCharacter = null;
            string newCharacter = null;
            string emotion = null;
            string emotionAmount = null;

            foreach (string line in info.Lines) {
                Match knotMatch = InkEditorUtils.KnotRegex.Match(line);

                if (knotMatch.Success) {
                    newKnot = knotMatch.Groups["knot"].Value;
                    continue;
                }

                Match stitchMatch = InkEditorUtils.StitchRegex.Match(line);

                if (stitchMatch.Success) {
                    newStitch = stitchMatch.Groups["stitch"].Value;
                    continue;
                }

                Match characterMatch = InkEditorUtils.CharacterRegex.Match(line);

                bool continueSearch = true;

                if (characterMatch.Success) {
                    newCharacter = characterMatch.Groups["character"].Value;
                    continueSearch = false;
                }

                Match emotionMatch = InkEditorUtils.EmotionRegex.Match(line);

                if (emotionMatch.Success) {
                    emotion = emotionMatch.Groups["emotion"].Value;
                    emotionAmount = emotionMatch.Groups["amount"].Value;
                    continueSearch = false;
                }

                if (!continueSearch || string.IsNullOrEmpty(newKnot) || !line.Contains('#')) {
                    continue;
                }

                MatchCollection tagMatches = InkEditorUtils.TagRegex.Matches(line);

                foreach (Match match in tagMatches) {
                    if (!int.TryParse(match.Groups["tag"].Value, out int tagId)) {
                        continue;
                    }

                    if (currentKnot != newKnot) {
                        currentKnot = newKnot;
                        sb.AppendLine($"<h1>{currentKnot.ToSpacedName(true, false, false)} <span>({currentKnot})</span></h1>");
                        currentCharacter = null;
                    }

                    if (currentStitch != newStitch) {
                        currentStitch = newStitch;
                        sb.AppendLine($"<h2>{currentStitch.ToSpacedName(true, false, false)}</h2>");
                        currentCharacter = null;
                    }

                    if (currentCharacter != newCharacter) {
                        currentCharacter = newCharacter;
                        sb.AppendLine($"<p><b>{currentCharacter.ToUpper()}</b></p>");
                    }

                    AppendDialogLine(line);

                    emotion = null;

                    void AppendDialogLine(string text, string voTag = "") {
                        sb.Append("<p>");
                        if (emotion != null) {
                            sb.Append($"<span>({emotion}");

                            if (!emotionAmount.IsNullOrEmpty()) {
                                sb.Append($" {emotionAmount}%");
                            }

                            sb.Append(")</span> ");
                        }
                        sb.AppendLine($"{InkUtils.CleanupTranscript(text)} <span>({tagId:00}{voTag})</span></p>");
                    }
                }
            }
        }

        private static string GetScript() {
            InkUtils.RefreshTranscript();
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in InkUtils.TranscriptLookup) {
                if (kvp.Value.Contains('{')) {
                    string boyLine = InkUtils.GetTranscriptByKey(kvp.Key + "-boy");
                    sb.AppendLine($"{kvp.Key}-boy\t{boyLine}");

                    string girlLine = InkUtils.GetTranscriptByKey(kvp.Key + "-girl");
                    sb.AppendLine($"{kvp.Key}-girl\t{girlLine}");

                    string petLine = InkUtils.GetTranscriptByKey(kvp.Key + "-pet");
                    if (petLine != girlLine && !petLine.IsNullOrEmpty()) {
                        sb.AppendLine($"{kvp.Key}-pet\t{petLine}");
                    }
                } else {
                    sb.AppendLine($"{kvp.Key}\t{kvp.Value}");
                }
            }
            
            return sb.ToString();
        }

        private static string GetMissingLines() {
            StringBuilder sb = new StringBuilder();

            InkUtils.RefreshTranscript();

            foreach (KeyValuePair<string, string> kvp in InkUtils.TranscriptLookup) {
                if (HasVO(kvp.Key)) {
                    continue;
                }

                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            return sb.ToString();
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

            if (string.IsNullOrEmpty(clipTestPath) || File.Exists(clipTestPath)) {
                return;
            }

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

            Debug.LogWarning($"No lip sync data found for '{clipPath}'", clip);
        }

        private static bool HasMissingData(string path) {
            if (!path.Contains("_VO")) {
                return false;
            }

            string lipSyncPath = Path.ChangeExtension(path, ".asset");
            return !string.IsNullOrEmpty(lipSyncPath) && !File.Exists(lipSyncPath);
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
            if (string.IsNullOrEmpty(lipSync.transcript) && !string.IsNullOrEmpty(transcript)) {
                Debug.LogWarning($"LipSync file's transcript is missing on {InkUtils.GetFolderName(filePath)}.{lipSync.name}: {transcript}", lipSync);
            }

            if (!lipSync.clip) {
                Debug.LogWarning($"Lip sync {lipSync.name} doesn't have a clip assigned.", lipSync);
            }

            string clipTestPath = Path.ChangeExtension(clipPath, ".asset");

            if (!string.IsNullOrEmpty(lipSyncPath) && !string.IsNullOrEmpty(clipTestPath) && !lipSyncPath.Equals(clipTestPath, StringComparison.OrdinalIgnoreCase)) {
                Debug.LogWarning($"Lip sync file '{lipSyncPath}' doesn't match clip '{clipPath}'", lipSync);
            }
        }

        private static void RenameFiles(string oldPath, string newPath) {
            if (File.Exists(newPath)) {
                Debug.LogError($"Can't rename {oldPath} -> {newPath} because new path exists.");
                return;
            }

            AssetDatabase.RenameAsset(oldPath, Path.GetFileName(newPath));
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

                    if (string.IsNullOrEmpty(tempData.transcript)) {
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
            if (transcript == null) {
                return null;
            }

            // Remove any <br/> html markup.
            if (!string.IsNullOrEmpty(transcript) && transcript.Contains('<')) {
                transcript = transcript.Replace(@"<br/>", " ");
                transcript = transcript.ReplaceRegex(@"<[^>]*>", "");
            }
            
            // Remove any single quotes.
            transcript = transcript.ReplaceRegex(@"('\B|\B')", "");

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