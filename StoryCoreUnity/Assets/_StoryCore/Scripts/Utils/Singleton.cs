using UnityEngine;

namespace StoryCore.Utils {
    /// <summary>
    ///     Be aware this will not prevent a non singleton constructor
    ///     such as `T myT = new T();`
    ///     To prevent that, add `protected T () {}` to your singleton class.<br />
    ///     NOTE: If there is a prefab with the same name as T,
    ///     it will be used instead of an empty GameObject.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T s_Instance;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object s_Lock = new object();

        public static bool Exists => AppTracker.IsPlaying && !AppTracker.IsQuitting;

        public static T Instance {
            get {
                if (!AppTracker.IsPlaying) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                if (AppTracker.IsQuitting) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (s_Lock) {
                    if (s_Instance == null) {
                        s_Instance = (T) FindObjectOfType(typeof(T));

#if DEBUG
                        if (FindObjectsOfType(typeof(T)).Length > 1) {
                            Debug.LogError("[Singleton] Something went really wrong " + " - there should never be more than 1 singleton!" + " Reopening the scene might fix it.");
                            return s_Instance;
                        }
#endif

                        if (s_Instance == null) {
                            // Support a prefab in resources that has the same name as the singleton being created.
                            GameObject singletonPrefab = Resources.Load<GameObject>(typeof(T).Name);
                            GameObject singleton = singletonPrefab ? Instantiate(singletonPrefab) : new GameObject(typeof(T).Name);
                            singleton.name = typeof(T).Name + " (singleton)";
                            s_Instance = singleton.GetOrAddComponent<T>();
                            DontDestroyOnLoad(s_Instance.gameObject);
                        } else {
                            //Debug.Log("[Singleton] Using instance already created: " + s_Instance.name);
                        }
                    }

                    return s_Instance;
                }
            }
        }
    }
}