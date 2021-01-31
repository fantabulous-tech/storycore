using CoreUtils;
using StoryCore.Commands;
using UnityEngine;

namespace StoryCore.Characters {
    public abstract class BaseCharacter : MonoBehaviour {
        [SerializeField] private string m_NameOverride;

        public string Name => m_NameOverride.IsNullOrEmpty() ? name : m_NameOverride;

        public abstract Transform AttentionPoint { get; }
        public virtual Transform SubtitlePoint => AttentionPoint;

        public abstract DelaySequence Perform(ScriptCommandInfo command);

        public abstract DelaySequence SetEmotion(ScriptCommandInfo command);

        protected virtual void OnEnable() {
            if (Buckets.Exists) {
                Buckets.Characters.Add(this);
            }
        }

        protected virtual void OnDisable() {
            if (Buckets.Exists) {
                Buckets.Characters.Remove(this);
            }
        }

        public abstract void LookAt(Transform target);
    }
}