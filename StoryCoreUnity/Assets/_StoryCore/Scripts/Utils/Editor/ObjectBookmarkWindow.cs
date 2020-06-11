using System;
using System.Collections.Generic;
using System.IO;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.Editor {
    /// <summary>
    ///     This window stores references to scene object and project objects in the player preferences as strings.
    ///     If the object exists then a button is presented that allows you to reselect it.
    /// </summary>
    public class ObjectBookmarkWindow : EditorWindow {
        private Vector2 m_ScrollPosition;
        private float m_WindowWidth;
        private List<ObjectBookmark> m_Bookmarks;

        private const string kPrefsKeyBookmarkItemCount = "ObjectBookmarkWindow_ItemCount";
        private const string kPrefsKeyBookMarkIsSceneObjectFormat = "ObjectBookmarkWindow_Item{0}_IsSceneObject";
        private const string kPrefsKeyBookMarkObjectGUIDFormat = "ObjectBookmarkWindow_Item{0}_GUID";
        private const string kPrefsKeyBookMarkObjectScenePathFormat = "ObjectBookmarkWindow_Item{0}_ScenePath";

        private const int kMaxDisplayStringLength = 32;
        private static readonly GUILayoutOption[] s_ButtonLayoutOptions = {GUILayout.Width(22), GUILayout.Height(19)};
        private static readonly GUILayoutOption[] s_SmallLayoutLayoutOptions = {GUILayout.Width(32), GUILayout.Height(19)};

        private static string m_ProjectKeyPrefix;

        private static string ProjectKeyPrefix => UnityUtils.GetOrSet(ref m_ProjectKeyPrefix, () => Path.GetFileName(Directory.GetCurrentDirectory()) + "_");

        private static string KeyItemCount = ProjectKeyPrefix + kPrefsKeyBookmarkItemCount;
        private static string KeyIsSceneObjectFormat = ProjectKeyPrefix + kPrefsKeyBookMarkIsSceneObjectFormat;
        private static string KeyObjectGUIDFormat = ProjectKeyPrefix + kPrefsKeyBookMarkObjectGUIDFormat;
        private static string KeyObjectScenePathFormat = ProjectKeyPrefix + kPrefsKeyBookMarkObjectScenePathFormat;

        [MenuItem("Window/Object Bookmarks")]
        public static void OpenWindow() {
            ObjectBookmarkWindow window = (ObjectBookmarkWindow) GetWindow(typeof(ObjectBookmarkWindow), false, "Bookmarks");
            window.Show();
        }

        private void Awake() {
            LoadProfileFromEditorPrefs();
        }

        public void OnGUI() {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            bool dimensionsChanged = false;

            if (Math.Abs(m_WindowWidth - position.width) > 1) {
                m_WindowWidth = position.width;
                dimensionsChanged = true;
            }

            if (m_Bookmarks == null) {
                LoadProfileFromEditorPrefs();
            }

            ObjectBookmark toDelete = null;

            foreach (ObjectBookmark bookmark in m_Bookmarks) {
                if (dimensionsChanged) {
                    bookmark.UpdateDisplayString(m_WindowWidth);
                }

                if (bookmark.OnGUI()) {
                    toDelete = bookmark;
                }
            }

            if (toDelete != null) {
                m_Bookmarks.Remove(toDelete);
                SaveProfile();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Add", s_SmallLayoutLayoutOptions);
            Object obj = EditorGUILayout.ObjectField(null, typeof(Object), true);
            if (obj != null) {
                AddBookmark(obj);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void AddBookmark(Object obj) {
            if (EditorUtility.IsPersistent(obj)) {
                // Asset database object
                ObjectBookmark newBookmark = new AssetBookmark(obj);
                newBookmark.UpdateDisplayString(m_WindowWidth);
                m_Bookmarks.Add(newBookmark);
            } else {
                GameObject go = obj as GameObject;
                if (go) {
                    // Scene object
                    ObjectBookmark newBookmark = new SceneBookmark(go);
                    newBookmark.UpdateDisplayString(m_WindowWidth);
                    m_Bookmarks.Add(newBookmark);
                } else {
                    Debug.Log("Can't create a bookmark for a scene object that isn't a GameObject");
                }
            }

            SaveProfile();
        }

        private void LoadProfileFromPlayerPrefs() {
            m_ScrollPosition = new Vector2(0.0f, 0.0f);
            m_Bookmarks = new List<ObjectBookmark>();

            int itemCount = 0;
            if (PlayerPrefs.HasKey(kPrefsKeyBookmarkItemCount)) {
                itemCount = PlayerPrefs.GetInt(kPrefsKeyBookmarkItemCount);
            }

            for (int i = 0; i < itemCount; ++i) {
                string isSceneObjectProfileKey = string.Format(kPrefsKeyBookMarkIsSceneObjectFormat, i);
                string objectGUIDProfileKey = string.Format(kPrefsKeyBookMarkObjectGUIDFormat, i);
                string objectScenePathProfileKey = string.Format(kPrefsKeyBookMarkObjectScenePathFormat, i);

                if (PlayerPrefs.HasKey(isSceneObjectProfileKey)) {
                    bool isSceneObject = PlayerPrefs.GetInt(isSceneObjectProfileKey) == 1;
                    ObjectBookmark newBookmark;

                    if (isSceneObject) {
                        string scenePath = PlayerPrefs.GetString(objectScenePathProfileKey);
                        newBookmark = new SceneBookmark(scenePath);
                    } else {
                        string guid = PlayerPrefs.GetString(objectGUIDProfileKey);
                        newBookmark = new AssetBookmark(guid);
                    }

                    newBookmark.UpdateDisplayString(m_WindowWidth);
                    m_Bookmarks.Add(newBookmark);
                }
            }
        }

        private void LoadProfileFromEditorPrefs() {
            m_ScrollPosition = new Vector2(0.0f, 0.0f);
            m_Bookmarks = new List<ObjectBookmark>();

            int itemCount = 0;
            if (EditorPrefs.HasKey(KeyItemCount)) {
                itemCount = EditorPrefs.GetInt(KeyItemCount);
            } else {
                LoadProfileFromPlayerPrefs();
                SaveProfile();
                return;
            }

            for (int i = 0; i < itemCount; ++i) {
                string isSceneObjectProfileKey = string.Format(KeyIsSceneObjectFormat, i);
                string objectGUIDProfileKey = string.Format(KeyObjectGUIDFormat, i);
                string objectScenePathProfileKey = string.Format(KeyObjectScenePathFormat, i);

                if (EditorPrefs.HasKey(isSceneObjectProfileKey)) {
                    bool isSceneObject = EditorPrefs.GetInt(isSceneObjectProfileKey) == 1;
                    ObjectBookmark newBookmark;

                    if (isSceneObject) {
                        string scenePath = EditorPrefs.GetString(objectScenePathProfileKey);
                        newBookmark = new SceneBookmark(scenePath);
                    } else {
                        string guid = EditorPrefs.GetString(objectGUIDProfileKey);
                        newBookmark = new AssetBookmark(guid);
                    }

                    newBookmark.UpdateDisplayString(m_WindowWidth);
                    m_Bookmarks.Add(newBookmark);
                }
            }
        }

        private void SaveProfile() {
            EditorPrefs.SetInt(KeyItemCount, m_Bookmarks.Count);

            for (int i = 0; i < m_Bookmarks.Count; ++i) {
                string isSceneObjectProfileKey = string.Format(KeyIsSceneObjectFormat, i);
                string objectGUIDProfileKey = string.Format(KeyObjectGUIDFormat, i);
                string objectScenePathProfileKey = string.Format(KeyObjectScenePathFormat, i);

                EditorPrefs.SetInt(isSceneObjectProfileKey, m_Bookmarks[i] is SceneBookmark ? 1 : 0);
                EditorPrefs.SetString(objectGUIDProfileKey, m_Bookmarks[i].ItemRef);
                EditorPrefs.SetString(objectScenePathProfileKey, m_Bookmarks[i].ItemRef);
            }
        }

        private abstract class ObjectBookmark {
            protected string m_DisplayString;

            protected abstract string FullName { get; }
            protected abstract float ButtonPadding { get; }
            public abstract string ItemRef { get; }

            public void UpdateDisplayString(float windowWidth) {
                m_DisplayString = FullName;

                if (windowWidth > 0) {
                    string trimmedString = m_DisplayString;
                    float currentWidth = GUI.skin.label.CalcSize(new GUIContent(m_DisplayString)).x;
                    int startIndex = 0;
                    while (currentWidth > windowWidth - ButtonPadding && startIndex < m_DisplayString.Length) {
                        ++startIndex;
                        trimmedString = "..." + m_DisplayString.Substring(startIndex, m_DisplayString.Length - startIndex);
                        currentWidth = GUI.skin.label.CalcSize(new GUIContent(trimmedString)).x;
                    }

                    m_DisplayString = trimmedString;
                } else {
                    if (m_DisplayString.Length > kMaxDisplayStringLength) {
                        m_DisplayString = "..." + m_DisplayString.Substring(m_DisplayString.Length - kMaxDisplayStringLength, kMaxDisplayStringLength);
                    }
                }
            }

            public abstract bool OnGUI();
        }

        private class AssetBookmark : ObjectBookmark {
            private readonly string m_GUID;

            protected override string FullName => AssetDatabase.GUIDToAssetPath(m_GUID);
            protected override float ButtonPadding => 100;
            public override string ItemRef => m_GUID;

            public AssetBookmark(Object obj) {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                m_GUID = AssetDatabase.AssetPathToGUID(assetPath);
            }

            public AssetBookmark(string guid) {
                m_GUID = guid;
            }

            public override bool OnGUI() {
                bool toDelete = false;

                EditorGUILayout.BeginHorizontal();

                string assetPath = AssetDatabase.GUIDToAssetPath(m_GUID);
                GUI.enabled = assetPath != null;
                // GUILayout.Label(EditorGUIUtility.IconContent("Collab.FileMoved", "Select asset in Project Window."), s_IconLayoutOptions);
                if (GUILayout.Button(new GUIContent(m_DisplayString, "Select asset in Project Window."))) {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }

                GUI.enabled = true;

                if (GUILayout.Button(new GUIContent("↗", "Open asset."), s_ButtonLayoutOptions)) {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
                }

                if (GUILayout.Button(new GUIContent("X", "Remove bookmark."), s_ButtonLayoutOptions)) {
                    toDelete = true;
                }

                EditorGUILayout.EndHorizontal();
                return toDelete;
            }
        }

        private class SceneBookmark : ObjectBookmark {
            private readonly string m_ScenePath;

            protected override string FullName => m_ScenePath;
            protected override float ButtonPadding => 70;
            public override string ItemRef => m_ScenePath;

            public SceneBookmark(GameObject go) {
                m_ScenePath = GetGameObjectPath(go.transform);
            }

            public SceneBookmark(string scenePath) {
                m_ScenePath = scenePath;
            }

            public override bool OnGUI() {
                bool deleteMe = false;
                EditorGUILayout.BeginHorizontal();

                // GUILayout.Label(EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow", "Scene object bookmark."), s_IconLayoutOptions);
                GameObject displayObject = GameObject.Find(m_ScenePath);
                GUI.enabled = displayObject != null;

                if (GUILayout.Button(new GUIContent(m_DisplayString, "Select object in Hierarchy Window."))) {
                    Selection.activeObject = displayObject;
                }

                GUI.enabled = true;

                if (GUILayout.Button(new GUIContent("X", "Remove bookmark."), s_ButtonLayoutOptions)) {
                    deleteMe = true;
                }

                EditorGUILayout.EndHorizontal();
                return deleteMe;
            }

            private static string GetGameObjectPath(Transform transform) {
                string path = transform.name;

                while (transform.parent != null) {
                    transform = transform.parent;
                    path = transform.name + "/" + path;
                }

                return path;
            }
        }
    }
}