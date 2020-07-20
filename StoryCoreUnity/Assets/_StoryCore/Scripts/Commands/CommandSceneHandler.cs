using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using VRSubtitles;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Scene")]
    public class CommandSceneHandler : CommandHandler {
        private static CommandSceneHandler m_Instance;
        
        [SerializeField] private AudioMixer m_Audio;
        [SerializeField] private AudioMixerSnapshot m_Normal;
        [SerializeField] private AudioMixerSnapshot m_Faded;
        [SerializeField] private StoryTellerLocator m_StoryTellerLocator;
        [SerializeField] private SceneBucket m_SceneBucket;

        private readonly List<SceneActionInfo> m_LoadOperations = new List<SceneActionInfo>();
        private readonly List<SceneActionInfo> m_UnloadOperations = new List<SceneActionInfo>();
        private string[] m_NextSceneList;

        protected void OnEnable() {
            if (!m_Instance) {
                m_Instance = this;
            } else {
                Debug.LogWarning("Duplicate CommandSceneHandler found.", this);
            }
        }

        public override DelaySequence Run(ScriptCommandInfo info) {
            // TODO: Cancel a scene load currently in progress?
            return LoadSceneInternal(info.Params[0]);
        }

        private DelaySequence LoadSceneInternal(string sceneName) {
            m_LoadOperations.Clear();
            m_UnloadOperations.Clear();
            StoryDebug.Log("Load scene " + sceneName + ": START");
            m_NextSceneList = GetSceneList(sceneName);

            if (Time.time.Approximately(0)) {
                return Delay.Until(UnloadedOldScene, this).ThenWaitUntil(LoadedNewScene).Then(FadeIn);
            }
            return Delay.WaitFor(FadeOut, this).ThenWaitUntil(UnloadedOldScene).ThenWaitUntil(LoadedNewScene).Then(FadeIn);
        }

        public static DelaySequence LoadScene(string sceneName) {
            if (!m_Instance) {
                Debug.LogError($"Couldn't find CommandSceneHandler. Can't load {sceneName}");
                return DelaySequence.Empty;
            }

            return m_Instance.LoadSceneInternal(sceneName);
        }

        private DelaySequence FadeOut() {
            float half = HeadsetFade.Duration/2;
            // Delay.For(half, this).Then(() => m_Audio.TransitionToSnapshots(new[] {m_Faded}, new[] {1f}, half));
            return HeadsetFade.FadeOut();
        }

        private void FadeIn() {
            // Clear old subtitles after we move.
            SubtitleDirector.Clear();

            // m_Audio.TransitionToSnapshots(new[] {m_Normal}, new[] {1f}, HeadsetFade.Duration / 2);
            HeadsetFade.FadeIn();
        }

        private void Save(string scene) {
            if (!PlayerPrefs.HasKey("SaveSlot")) {
                PlayerPrefs.SetInt("SaveSlot", 0);
            }
            int slot = PlayerPrefs.GetInt("SaveSlot", 0);
            string[] data = {m_StoryTellerLocator.Value.Story.state.ToJson(), scene};
            string path = Application.persistentDataPath + "/Save";
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            BinaryFormatter b = new BinaryFormatter();
            FileStream file = File.Create(path + "/Slot" + slot);
            b.Serialize(file, data);
            file.Close();
            //Debug.Log("Save to slot "+slot+" at "+path);
        }

        private class SceneActionInfo {
            public string Name { get; }
            private AsyncOperation Operation { get; }

            public bool Done => Operation.isDone;

            public SceneActionInfo(string name, AsyncOperation operation) {
                Name = name;
                Operation = operation;
            }
        }

        private bool UnloadedOldScene() {
            try {
                if (m_UnloadOperations.Count == 0) {
                    foreach (string path in m_SceneBucket.Items) {
                        string sceneName = Path.GetFileNameWithoutExtension(path);

                        // Don't unload scenes that we want in the next scene.
                        if (sceneName == null || m_NextSceneList.Contains(sceneName, StringComparer.OrdinalIgnoreCase)) {
                            continue;
                        }

                        Scene oldScene = SceneManager.GetSceneByName(sceneName);
                        if (oldScene.isLoaded) {
                            StoryDebug.Log("Unload scene " + sceneName + ": START");
                            m_UnloadOperations.Add(new SceneActionInfo(sceneName, SceneManager.UnloadSceneAsync(oldScene)));
                        }
                    }
                }

                for (int i = m_UnloadOperations.Count - 1; i >= 0; i--) {
                    SceneActionInfo sceneUnload = m_UnloadOperations[i];
                    if (sceneUnload.Done) {
                        StoryDebug.Log("Unload scene " + sceneUnload.Name + ": COMPLETE");
                        m_UnloadOperations.Remove(sceneUnload);
                    }
                }

                return m_UnloadOperations.Count == 0;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return true;
            }
        }

        private bool LoadedNewScene() {
            try {
                if (m_LoadOperations.Count == 0) {
                    foreach (string sceneName in m_NextSceneList) {
                        string path = m_SceneBucket.Items.FirstOrDefault(p => p != null && Path.GetFileNameWithoutExtension(p).Equals(sceneName, StringComparison.OrdinalIgnoreCase));
                        if (path != null) {
                            // If the scene is already loaded, skip it.
                            if (SceneManager.GetSceneByName(sceneName).IsValid()) {
                                continue;
                            }

                            StoryDebug.Log("Load scene " + sceneName + ": START");
                            m_LoadOperations.Add(new SceneActionInfo(sceneName, SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)));
                        } else {
                            Debug.LogWarningFormat(this, "Could not load scene '" + sceneName + "' as it was not found.");
                        }
                    }
                }

                for (int i = m_LoadOperations.Count - 1; i >= 0; i--) {
                    SceneActionInfo sceneLoad = m_LoadOperations[i];
                    if (sceneLoad.Done) {
                        StoryDebug.Log("Load scene " + sceneLoad.Name + ": COMPLETE");
                        m_LoadOperations.Remove(sceneLoad);
                    }
                }

                bool isLoaded = m_LoadOperations.Count == 0;

                if (isLoaded) {
                    string firstSceneName = m_NextSceneList.FirstOrDefault();

                    if (!firstSceneName.IsNullOrEmpty()) {
                        Scene firstScene = SceneManager.GetSceneByName(firstSceneName);

                        if (firstScene.isLoaded) {
                            SceneManager.SetActiveScene(firstScene);
                        } else {
                            Debug.LogError($"No first scene found for {firstSceneName}", this);
                        }
                    } else {
                        Debug.LogError($"No first scene name found in scene list: {m_NextSceneList.AggregateToString()}", this);
                    }
                }

                return isLoaded;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return true;
            }
        }

        private static string[] GetSceneList(string fullSceneName) {
            string sceneName = null;
            string[] pieces = fullSceneName.Split('.');
            string[] sceneList = new string[pieces.Length];
            for (int i = 0; i < pieces.Length; i++) {
                sceneList[i] = sceneName = sceneName.IsNullOrEmpty() ? pieces[i] : sceneName + "." + pieces[i];
            }
            return sceneList;
        }
    }
}