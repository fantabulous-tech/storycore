using UnityEngine;

namespace StoryCore {
    public static class StoryDebug {
        private const string kColorCode = "#40ACFF";

        public static void Log(string message) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.Log($"<color={kColorCode}>SC:</color> {message}");
            }
        }

        public static void Log(string message, Object context) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.Log($"<color={kColorCode}>SC:</color> {message}", context);
            }
        }

        public static void LogFormat(string message, params object[] args) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.LogFormat($"<color={kColorCode}>SC:</color> {message}", args);
            }
        }

        public static void LogFormat(Object context, string message, params object[] args) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.LogFormat(context, $"<color={kColorCode}>SC:</color> {message}", args);
            }
        }
    }
}