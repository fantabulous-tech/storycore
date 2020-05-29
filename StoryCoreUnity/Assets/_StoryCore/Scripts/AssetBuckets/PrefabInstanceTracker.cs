using System;
using UnityEngine;

namespace StoryCore.AssetBuckets {
    public class PrefabInstanceTracker : MonoBehaviour {
        public event Action<GameObject> Destroyed;

        private void OnDestroy() {
            Destroyed?.Invoke(gameObject);
        }
    }
}