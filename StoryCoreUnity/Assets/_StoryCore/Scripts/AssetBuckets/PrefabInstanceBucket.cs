using System;
using System.Collections.Generic;
using StoryCore.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.AssetBuckets {
    public abstract class PrefabInstanceBucket<T> : GenericAssetBucket<T> where T : Object {
        private Dictionary<string, T> m_Instances;

        public Dictionary<string, T> Instances => UnityUtils.GetOrSet(ref m_Instances, () => new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase));

        public event Action<T> Added;
        public event Action<T> Removed;

        public void Add(T instance) {
            Instances[instance.name] = instance;
            PrefabInstanceTracker tracker = UnityUtils.GetGameObject(instance).GetOrAddComponent<PrefabInstanceTracker>();
            tracker.Destroyed += OnInstanceDestroyed;
            Added?.Invoke(instance);
        }

        private void OnInstanceDestroyed(GameObject obj) {
            Remove(obj.name);
        }

        public void Remove(T instance) {
            if (!instance || !Instances.ContainsKey(instance.name)) {
                return;
            }

            m_Instances.Remove(instance.name);
            Removed?.Invoke(instance);
        }

        private T Remove(string assetName) {
            if (!m_Instances.TryGetValue(assetName, out T instance)) {
                return null;
            }

            m_Instances.Remove(assetName);
            Removed?.Invoke(instance);
            return instance;
        }

        public void RemoveAndDestroy(string assetName) {
            T instance = Remove(assetName);

            if (instance) {
                Destroy(instance);
            }
        }

        public override T Get(string assetName) {
            if (assetName.IsNullOrEmpty()) {
                return null;
            }
            Instances.TryGetValue(assetName, out T instance);
            return instance;
        }

        public T GetOrCreate(string assetName) {
            T instance = Get(assetName);

            if (!instance) {
                T prefab = base.Get(assetName);

                if (!prefab) {
                    return null;
                }

                Instances[assetName] = Instantiate(prefab);
                return Instances[assetName];
            }

            return null;
        }

#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            if (!asset) {
                return false;
            }

            UnityEditor.PrefabAssetType prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(asset);
            bool rightType = prefabType == UnityEditor.PrefabAssetType.Regular || prefabType == UnityEditor.PrefabAssetType.Variant;
            return rightType && base.EDITOR_CanAdd(asset);
        }
#endif
    }
}