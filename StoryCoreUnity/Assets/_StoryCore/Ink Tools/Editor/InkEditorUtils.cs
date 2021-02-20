using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CoreUtils;
using CoreUtils.AssetBuckets;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using StoryCore.InkTodo;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.Utils {
    public static class InkEditorUtils {
        public const string kStoryScriptPath = "Assets/_Stories/_game.ink";

        public static readonly Regex TagRegex = new Regex(@"^(?!\/\/).*#(?<tag>[0-9]+)(\s|$)");
        public static readonly Regex KnotRegex = new Regex(@"^\s*=[=]+\s*(?<knot>\w+)");
        public static readonly Regex StitchRegex = new Regex(@"^\s*=\s*(?<stitch>\w+)");
        public static readonly Regex CharacterRegex = new Regex(@"^[\s-]*(\([^\)]+\))?[\s-]*\/character\s+(?<character>\w+).*$");
        public static readonly Regex EmotionRegex = new Regex(@"(\/emotion|\/perform\s+\w+|\/character\s+\w+\s+\w+)\s+(?<emotion>\w+)(\s+(?<amount>[0-9]+))?.*$");

        public delegate string ScriptInterpreter(string scriptText, string postFix, Object context);

        private static BaseBucket s_VOBucket;
        private static BaseBucket VOBucket {
            get {
                if (s_VOBucket == null) {
                    s_VOBucket = AssetDatabase.LoadAssetAtPath<BaseBucket>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:VOBucket").First()));
                }

                return s_VOBucket;
            }
        }

        [MenuItem("Assets/Check Ink Lines", false, 70)]
        public static void CheckInkLines() {
            foreach (Object obj in Selection.objects) {
                if (!obj) {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                string fileId = GetFileId(path);
                string[] lines = File.ReadAllLines(path);
                Dictionary<int, HashSet<int>> tagIds = GatherTags(fileId, lines, obj);

                if (!TagUntaggedLines(fileId, lines, tagIds, obj)) {
                    Debug.Log($"{fileId}: No tags needed.", obj);
                }
            }
        }

        [MenuItem("Assets/Tag Ink Lines", false, 70)]
        public static void TagInkLines() {
            foreach (Object obj in Selection.objects) {
                if (!obj) {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                string ext = Path.GetExtension(path);

                if (ext == ".ink" || ext.IsNullOrEmpty()) {
                    TagInkLinesAtPath(path, obj);
                }
            }
        }

        public static void TagInkLinesAtPath(string path, Object context) {
            string fileId = GetFileId(path);
            string[] lines = File.ReadAllLines(path);
            Dictionary<int, HashSet<int>> tagIds = GatherTags(fileId, lines, context);

            if (!TagUntaggedLines(fileId, lines, tagIds, context)) {
                Debug.Log($"{fileId}: No tags needed.", context);
                return;
            }

            SaveLines(lines, path);
        }

        [MenuItem("Assets/Clear Ink Tags", false, 71)]
        public static void ClearInkTags() {
            foreach (Object obj in Selection.objects) {
                if (!obj) {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                string fileId = GetFileId(path);
                string[] lines = File.ReadAllLines(path);
                bool changed = RemoveTags(fileId, lines);
                if (changed) {
                    SaveLines(lines, path);
                } else {
                    Debug.Log($"{fileId}: No tags found.");
                }
            }
        }

        [MenuItem("Assets/Retag Ink Lines", false, 71)]
        public static void RetagInkLines() {
            foreach (Object obj in Selection.objects) {
                if (!obj) {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                string fileId = GetFileId(path);
                string[] lines = File.ReadAllLines(path);
                bool changed = RemoveTags(fileId, lines);
                changed = TagUntaggedLines(fileId, lines) || changed;
                if (changed) {
                    SaveLines(lines, path);
                } else {
                    Debug.Log($"{fileId}: No tags needed.");
                }
            }
        }

        public static string GetFileId(string path) {
            string name = Path.GetFileNameWithoutExtension(path);
            string folder = Path.GetDirectoryName(path).Split('\\', '/').Last();
            return folder + "/" + name;
        }

        private static void SaveLines(string[] lines, string path) {
            if (Provider.isActive) {
                Provider.Checkout(Selection.activeObject, CheckoutMode.Both).Wait();
            }
            File.WriteAllLines(path, lines);
            AssetDatabase.ImportAsset(path);
        }

        private static bool RemoveTags(string fileId, string[] lines) {
            int count = 0;

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i].ReplaceRegex(@"\s*#(?<tag>[0-9]+)(\s*|$)", "");
                if (lines[i] == line) {
                    continue;
                }
                lines[i] = line;
                count++;
            }

            if (count > 0) {
                Debug.Log($"Removed {count} tags from {fileId}.");
            }

            return count > 0;
        }

        private static bool TagUntaggedLines(string fileId, string[] lines, Dictionary<int, HashSet<int>> tagSets = null, Object context = null) {
            tagSets = tagSets ?? new Dictionary<int, HashSet<int>>();

            int currentKnot = 0;
            string currentKnotName = null;
            HashSet<int> existingTags = GetTagSet(currentKnot, tagSets);

            int currentId = 1;
            bool changed = false;

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];

                Match knotMatch = KnotRegex.Match(line);
                if (knotMatch.Success) {
                    currentKnotName = knotMatch.Groups["knot"].Value;
                    currentKnot++;
                    currentId = 1;
                    existingTags = GetTagSet(currentKnot, tagSets);
                    continue;
                }

                if (!CanTag(line, fileId, i, context)) {
                    continue;
                }
                lines[i] = line.TrimEnd() + " #" + GetNextId(currentKnotName, ref currentId, existingTags).ToString("00");
                changed = true;
            }

            return changed;
        }

        private static HashSet<int> GetTagSet(int knotNumber, Dictionary<int, HashSet<int>> tagSets) {
            if (tagSets.ContainsKey(knotNumber)) {
                return tagSets[knotNumber];
            }
            return tagSets[knotNumber] = new HashSet<int>();
        }

        public static bool CanTag(string line) {
            return CanTag(line, null);
        }

        public static bool CanTag(string line, string fileId, int i = -1, Object context = null) {
            string originalLine = line;

            line = line.Trim();

            // If we already have a tag, then we can't tag.
            if (line.Contains('#') && TagRegex.IsMatch(line)) {
                return false;
            }

            // If the line has a TODO.
            if (line.Contains("TODO")) {
                return false;
            }

            // If the line has placeholder description
            if (line.Contains("<<")) {
                return false;
            }

            // Remove any '-> Ref' bits.
            line = line.ReplaceRegex(@"\-\>\s*[a-zA-Z0-9_.]+(\([^)]*\))?(\([\w\s,]*\))?", "");

            // Remove '- else:' and other conditionals that end with ':'
            line = line.ReplaceRegex(@"^\s*-\s*[^{""]*:", "", RegexOptions.IgnoreCase);

            // Skip '<blockquote>' passages used for the web stuff.
            if (line.ContainsRegex(@"^\s*<blockquote>", RegexOptions.IgnoreCase)) {
                return false;
            }

            if (line.StartsWith("+")) {
                return false;
            }

            // Remove labels
            line = line.ReplaceRegex(@"^\s*(\+|\-)+\s*(\(\s*[a-zA-Z0-9_.]+\s*\))?", "").Trim();

            // Ignore INCLUDE lines.
            if (line.StartsWith("INCLUDE")) {
                return false;
            }

            // Ignore EXTERNAL lines.
            if (line.StartsWith("EXTERNAL")) {
                return false;
            }

            // Ignore VAR lines.
            if (line.StartsWith("VAR")) {
                return false;
            }

            // Ignore LIST lines.
            if (line.StartsWith("LIST")) {
                return false;
            }

            // Ignore # instruction lines.
            if (line.StartsWith("#")) {
                return false;
            }

            // Skip to dos.
            if (line.ContainsRegex(@"^\s*TODO:")) {
                return false;
            }

            // Ignore code lines.
            if (line.StartsWith("~")) {
                return false;
            }

            // Ignore comments
            line = line.ReplaceRegex(@"\s*//.*", "");

            // Ignore knots and stitches.
            if (line.StartsWith("=")) {
                return false;
            }

            Regex ifCode = new Regex(@"{[^:}]+:([^:|}]*)\|(.*)}");

            // Replace 'if' code with first option.
            while (ifCode.IsMatch(line)) {
                line = ifCode.Replace(line, "$1");
            }

            // Remove in-line code
            line = line.ReplaceRegex(@"{[^}]*}", "");

            // Remove list type
            line = line.ReplaceRegex(@"{[^:]*:", "");

            // Remove redirects
            if (line.Contains("->")) {
                line = line.ReplaceRegex(@"->.*", "");
            }

            // Remove list logic (e.g. '- else:')
            line = line.ReplaceRegex(@"^\s*[-]+\s*[a-zA-Z0-9_]+\s*:", "");

            // Remove any remaining trim.
            line = line.Trim();

            // Ignore command lines
            if (line.StartsWith("/")) {
                return false;
            }

            // Remove choices since they don't have VO
            line = line.ReplaceRegex(@"(\+|\*)?\s*\[[^\]]*\]", "");

            // Return true if we have text remaining, otherwise false.
            bool result = line.ContainsRegex("[a-zA-Z]");

            if (result && !fileId.IsNullOrEmpty()) {
                Debug.Log($"{fileId} ({i + 1}): Missing a tag: {line}\nOriginal line: {originalLine}", context);
            }

            return result;
        }

        private static int GetNextId(string knotName, ref int currentId, ISet<int> existingTags) {
            while (existingTags.Contains(currentId) || VOBucket.Has(knotName + "." + currentId.ToString("00"))) {
                currentId++;
            }
            existingTags.Add(currentId);
            return currentId;
        }

        private static Dictionary<int, HashSet<int>> GatherTags(string fileId, IEnumerable<string> lines, Object context) {
            int currentKnot = 0;
            Dictionary<int, HashSet<int>> tagSets = new Dictionary<int, HashSet<int>>();
            HashSet<int> tags = GetTagSet(currentKnot, tagSets);

            foreach (string line in lines) {
                if (line.ContainsRegex(@"^\s*==")) {
                    currentKnot++;
                    tags = GetTagSet(currentKnot, tagSets);
                    continue;
                }

                if (!line.Contains('#')) {
                    continue;
                }

                MatchCollection matches = TagRegex.Matches(line);

                for (int i = 0; i < matches.Count; i++) {
                    Match match = matches[i];
                    if (int.TryParse(match.Groups["tag"].Value, out int tagId)) {
                        if (tags.Contains(tagId)) {
                            Debug.LogWarning($"{fileId} ({i}): Duplicate tag found: {tagId}", context);
                        } else {
                            tags.Add(tagId);
                        }
                    }
                }
            }

            return tagSets;
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

        private static Dictionary<string, string> GetTranscriptLookup() {
            Dictionary<string, string> transcriptLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<InkFileInfo> inkFiles = InkEditorUtils.GetInkFiles(kStoryScriptPath);

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

        public static string GetFolderName(string filePath) {
            string[] folderPieces = filePath.Split('/');
            return folderPieces.Length >= 2 ? folderPieces[folderPieces.Length - 2] : null;
        }

        public static void RefreshTranscript() {
            m_TranscriptLookup = GetTranscriptLookup();
        }

        public static string GetTranscript(string voFilePath, ScriptInterpreter interpreter, Object context = null) {
            string folderName = InkEditorUtils.GetFolderName(voFilePath);

            if (folderName.IsNullOrEmpty()) {
                Debug.LogWarning("Couldn't find folder name of " + voFilePath);
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(voFilePath);
            string id = fileName;
            int dashIndex = fileName.IndexOf('-');
            string postFix = null;

            if (dashIndex > 0) {
                postFix = fileName.Substring(dashIndex + 1);
                id = fileName.Substring(0, fileName.IndexOf('-'));
            }

            string lipSyncKey = folderName + "." + id;

            if (!InkEditorUtils.TranscriptLookup.TryGetValue(lipSyncKey, out string scriptText)) {
                LipSyncData data = context as LipSyncData;
                string transcriptInfo = data != null && !data.transcript.IsNullOrEmpty() ? $"({data.transcript})" : "(no transcript)";
                Debug.LogWarning($"Missing tagged ink line: {lipSyncKey}. {transcriptInfo}", context);
                return null;
            }

            if (interpreter != null) {
                scriptText = interpreter(scriptText, postFix, context);
            }

            return scriptText;
        }

        public static string GetTranscriptByKey(string key, ScriptInterpreter interpreter, Object context = null) {
            string[] keyPieces = key.Split('.');
            string folderName = keyPieces.FirstOrDefault();

            if (folderName.IsNullOrEmpty() || keyPieces.Length < 2) {
                Debug.LogWarning("Couldn't find folder name of " + key);
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(keyPieces[1]);
            string id = fileName;
            int dashIndex = fileName.IndexOf('-');
            string postFix = null;

            if (dashIndex > 0) {
                postFix = fileName.Substring(dashIndex + 1);
                id = fileName.Substring(0, fileName.IndexOf('-'));
            }

            string lipSyncKey = folderName + "." + id;

            if (!InkEditorUtils.TranscriptLookup.TryGetValue(lipSyncKey, out string scriptText)) {
                Debug.Log("No transcript found for " + lipSyncKey, context);
                return null;
            }

            if (interpreter != null) {
                scriptText = interpreter(scriptText, postFix, context);
            }

            return scriptText;
        }
    }
}