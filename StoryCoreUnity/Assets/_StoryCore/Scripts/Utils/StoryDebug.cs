using UnityEngine;

namespace StoryCore {
    public static class StoryDebug {
        public static void Log(string message) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.Log(message);
            }
        }

        public static void Log(string message, Object context) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.Log(message, context);
            }
        }

        public static void LogFormat(string message, params object[] args) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.LogFormat(message, args);
            }
        }

        public static void LogFormat(Object context, string message, params object[] args) {
            if (StoryCoreSettings.EnableLogging) {
                Debug.LogFormat(context, message, args);
            }
        }
    }
}