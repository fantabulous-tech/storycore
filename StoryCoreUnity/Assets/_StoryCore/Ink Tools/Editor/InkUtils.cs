using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CoreUtils;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.InkTodo {
    public static class InkUtils {
        public const string kStoryScriptPath = "Assets/_Stories/_game.ink";

        private static AutoSyncPreset s_AutoSyncPreset;
        private static AutoSyncModule[] s_AutoSyncModules;
        private static readonly Regex s_CleanTranscriptRegex = new Regex(@"^(\s*//\s*(-\s*)?|(\s*//)?\s*-\s*(\([^\)]*\)\s*)?|(\s*//)?\s*\+\s*\[[^\]]+\]\s*)(?<text>.*)");
        private static Dictionary<string, string> m_TranscriptLookup;
        private static string s_LanguageModelName;
        private static LipSyncProject s_Settings;

        public static AutoSyncModule[] AutoSyncModules {
            get {
                if (s_AutoSyncModules == null) {
                    s_AutoSyncModules = new AutoSyncModule[AutoSyncPreset.modules.Length];

                    for (int i = 0; i < AutoSyncPreset.modules.Length; i++) {
                        s_AutoSyncModules[i] = (AutoSyncModule) ScriptableObject.CreateInstance(AutoSyncPreset.modules[i]);

                        if (!string.IsNullOrEmpty(AutoSyncPreset.moduleSettings[i])) {
                            JsonUtility.FromJsonOverwrite(AutoSyncPreset.moduleSettings[i], s_AutoSyncModules[i]);
                        }
                    }
                }

                return s_AutoSyncModules;
            }
        }

        private static AutoSyncPreset AutoSyncPreset {
            get {
                if (s_AutoSyncPreset == null) {
                    s_AutoSyncPreset = AutoSyncUtility.GetPresets().First(p => p.name.StartsWith("Default", StringComparison.OrdinalIgnoreCase));
                }

                return s_AutoSyncPreset;
            }
        }

        public static Dictionary<string, string> TranscriptLookup {
            get {
                if (m_TranscriptLookup == null) {
                    m_TranscriptLookup = GetTranscriptLookup();
                }

                return m_TranscriptLookup;
            }
        }

        public static List<InkFileInfo> GetInkFiles(string storyScriptPath) {
            string rootPath = Path.GetDirectoryName(storyScriptPath).Replace('\\', '/').TrimEnd('/');
            DefaultAsset rootInkFile = AssetDatabase.LoadAssetAtPath<DefaultAsset>(storyScriptPath);
            InkFileInfo rootInfo = new InkFileInfo(rootInkFile, rootPath);

            Queue<InkFileInfo> queue = new Queue<InkFileInfo>(new[] {rootInfo});
            List<InkFileInfo> inkFiles = new List<InkFileInfo>();

            while (queue.Count > 0) {
                InkFileInfo info = queue.Dequeue();
                inkFiles.Add(info);

                foreach (string includePath in info.Includes) {
                    if (inkFiles.Any(f => f.Path.Equals(includePath, StringComparison.OrdinalIgnoreCase))) {
                        continue;
                    }
                    queue.Enqueue(new InkFileInfo(includePath, rootPath));
                }
            }

            return inkFiles;
        }

        private static Dictionary<string, string> GetTranscriptLookup() {
            Dictionary<string, string> transcriptLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<InkFileInfo> inkFiles = GetInkFiles(kStoryScriptPath);

            string currentKnot = null;

            foreach (InkFileInfo info in inkFiles) {
                for (int i = 0; i < info.Lines.Length; i++) {
                    string line = info.Lines[i];

                    Match knotMatch = InkEditorUtils.KnotRegex.Match(line);

                    if (knotMatch.Success) {
                        currentKnot = knotMatch.Groups["knot"].Value;
                        continue;
                    }

                    if (currentKnot.IsNullOrEmpty() || !line.Contains('#')) {
                        continue;
                    }

                    MatchCollection tagMatches = InkEditorUtils.TagRegex.Matches(line);

                    for (int j = 0; j < tagMatches.Count; j++) {
                        Match match = tagMatches[j];
                        if (int.TryParse(match.Groups["tag"].Value, out int tagId)) {
                            string result = currentKnot + "." + tagId.ToString("00");
                            string script = CleanupTranscript(line);
                            if (transcriptLookup.TryGetValue(result, out string previousScript) && previousScript != script) {
                                Debug.LogWarning($"Duplicate VO reference found: {script} vs. {previousScript} in {info.DisplayPath}", info.InkAsset);
                            }
                            transcriptLookup[result] = script;
                        }
                    }
                }
            }
            return transcriptLookup;
        }

        public static string CleanupTranscript(string line) {
            int tagIndex = line.IndexOf("#", StringComparison.Ordinal);

            if (tagIndex > 0) {
                line = line.Substring(0, tagIndex);
            }

            Regex cleanUp = s_CleanTranscriptRegex;
            Match match = cleanUp.Match(line);

            if (match.Success) {
                line = match.Groups["text"].Value;
            }

            return line.Trim();
        }

        public static string GetTranscript(string voFilePath, Object context = null) {
            string folderName = GetFolderName(voFilePath);

            if (folderName.IsNullOrEmpty()) {
                Debug.LogWarning("Couldn't find folder name of " + voFilePath);
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(voFilePath);
            string id = fileName;
            int dashIndex = fileName.IndexOf('-');
            string postFix = null;

            if (dashIndex > 0) {
                id = fileName.Substring(0, fileName.IndexOf('-'));
            }

            string lipSyncKey = folderName + "." + id;

            if (!TranscriptLookup.TryGetValue(lipSyncKey, out string scriptText)) {
                LipSyncData data = context as LipSyncData;
                string transcriptInfo = data != null && !data.transcript.IsNullOrEmpty() ? $"({data.transcript})" : "(no transcript)";
                Debug.LogWarning($"Missing tagged ink line: {lipSyncKey}. {transcriptInfo}", context);
                return null;
            }

            return scriptText;
        }

        public static string GetRawTranscript(string voFilePath) {
            string folderName = GetFolderName(voFilePath);

            if (folderName.IsNullOrEmpty()) {
                Debug.LogWarning("Couldn't find folder name of " + voFilePath);
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(voFilePath);
            string id = fileName;
            int dashIndex = fileName.IndexOf('-');

            if (dashIndex > 0) {
                id = fileName.Substring(0, fileName.IndexOf('-'));
            }

            string lipSyncKey = folderName + "." + id;

            if (!TranscriptLookup.TryGetValue(lipSyncKey, out string scriptText)) {
                return null;
            }

            return scriptText;
        }
        
        public static string[] GetTagsFromPath(string voFilePath) {
            string fileName = Path.GetFileNameWithoutExtension(voFilePath);
            return fileName.Split(new []{'-', ' '}, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
        }

        public static string GetTranscriptByKey(string key, Object context = null) {
            string[] keyPieces = key.Split('.');
            string folderName = keyPieces.FirstOrDefault();

            if (folderName.IsNullOrEmpty() || keyPieces.Length < 2) {
                Debug.LogWarning("Couldn't find folder name of " + key);
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(keyPieces[1]);
            string id = fileName;
            int dashIndex = fileName.IndexOf('-');

            if (dashIndex > 0) {
                id = fileName.Substring(0, fileName.IndexOf('-'));
            }

            string lipSyncKey = folderName + "." + id;

            if (!TranscriptLookup.TryGetValue(lipSyncKey, out string scriptText)) {
                Debug.Log("No transcript found for " + lipSyncKey, context);
                return null;
            }

            return scriptText;
        }

        public static string GetFolderName(string filePath) {
            string[] folderPieces = filePath.Split('/');
            return folderPieces.Length >= 2 ? folderPieces[folderPieces.Length - 2] : null;
        }

        public static void RefreshTranscript() {
            m_TranscriptLookup = GetTranscriptLookup();
        }
    }
}