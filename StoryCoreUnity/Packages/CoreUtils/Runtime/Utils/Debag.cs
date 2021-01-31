using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CoreUtils {
    public static class Debag {
        private static readonly Dictionary<MethodBase, HashSet<int>> s_LogOnceLocations = new Dictionary<MethodBase, HashSet<int>>();

        private static bool CheckAlreadyLoggedFromLocation() {
            StackFrame stackFrame = new StackFrame(2);
            Dictionary<MethodBase, HashSet<int>> logOnceLocations = s_LogOnceLocations;
            bool alreadyLogged = false;
            bool locked = false;
            
            try {
                Monitor.Enter(logOnceLocations, ref locked);
                MethodBase method = stackFrame.GetMethod();
                
                if (!s_LogOnceLocations.TryGetValue(method, out HashSet<int> intSet)) {
                    intSet = new HashSet<int>();
                    s_LogOnceLocations.Add(method, intSet);
                }

                int ilOffset = stackFrame.GetILOffset();

                if (intSet.Contains(ilOffset)) {
                    alreadyLogged = true;
                } else {
                    intSet.Add(ilOffset);
                }
            }
            finally {
                if (locked) {
                    Monitor.Exit(logOnceLocations);
                }
            }

            return alreadyLogged;
        }

        public static void LogOnce(string message, Object context = null) {
            if (!CheckAlreadyLoggedFromLocation()) {
                Log(message, context);
            }
        }

        public static void LogWarningOnce(string message, Object context = null) {
            if (!CheckAlreadyLoggedFromLocation()) {
                LogWarning(message, context);
            }
        }

        public static void LogErrorOnce(string message, Object context = null) {
            if (!CheckAlreadyLoggedFromLocation()) {
                LogError(message, context);
            }
        }

        public static void Log(string message, Object context = null) {
            Debug.Log(message, context);
        }

        public static void LogWarning(string message, Object context = null) {
            Debug.LogWarning(message, context);
        }

        public static void LogError(string message, Object context = null) {
            Debug.LogError(message, context);
        }
    }
}