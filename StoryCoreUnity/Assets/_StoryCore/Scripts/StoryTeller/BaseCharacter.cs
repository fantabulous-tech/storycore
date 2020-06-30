using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    public abstract class BaseCharacter : MonoBehaviour {
        [SerializeField] private string m_NameOverride;

        public string Name => m_NameOverride.IsNullOrEmpty() ? name : m_NameOverride;

        public abstract Transform AttentionPoint { get; }
        public virtual Transform SubtitlePoint => AttentionPoint;

        public abstract DelaySequence Perform(ScriptCommandInfo command);

        public abstract DelaySequence SetEmotion(ScriptCommandInfo command);

        public abstract DelaySequence MoveTo(ScriptCommandInfo command);

        protected virtual void Awake() {
            Buckets.Characters.Add(this);
        }

        protected void OnDestroy() {
            if (Buckets.Exists) {
                Buckets.Characters.Remove(this);
            }
        }

        public abstract void LookAt(Transform target);
    }
}