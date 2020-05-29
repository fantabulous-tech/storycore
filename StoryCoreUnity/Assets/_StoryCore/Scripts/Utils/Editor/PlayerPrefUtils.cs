using UnityEditor;
using UnityEngine;

public static class PlayerPrefUtils {
    [MenuItem("Tools/Reset PlayerPrefs")]
    private static void DeleteAllPlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }
}