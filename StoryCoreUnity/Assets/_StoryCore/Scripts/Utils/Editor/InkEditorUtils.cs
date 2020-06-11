using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StoryCore.AssetBuckets;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace StoryCore.Utils {
    public static class InkEditorUtils {
        public static readonly Regex TagRegex = new Regex(@"^(?!\/\/).*#(?<tag>[0-9]+)(\s|$)");
        public static readonly Regex KnotRegex = new Regex(@"^\s*=[=]+\s*(?<knot>\w+)");

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
                string fileId = GetFileId(path);
                string[] lines = File.ReadAllLines(path);
                Dictionary<int, HashSet<int>> tagIds = GatherTags(fileId, lines, obj);

                if (!TagUntaggedLines(fileId, lines, tagIds, obj)) {
                    Debug.Log($"{fileId}: No tags needed.", obj);
                    continue;
                }

                SaveLines(lines, path);
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

        private static string GetFileId(string path) {
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

        public static bool CanTag(string line, string fileId = null, int i = -1, Object context = null) {
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

            // Ignore command lines
            if (line.StartsWith("/")) {
                return false;
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
    }
}