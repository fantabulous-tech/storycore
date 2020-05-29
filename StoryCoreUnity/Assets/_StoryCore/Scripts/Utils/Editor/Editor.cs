using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Utils {
    public abstract class AttributeDrawer<T> : PropertyDrawer where T : PropertyAttribute {
        private T m_Attribute;

        protected T Attribute => UnityUtils.GetOrSet(ref m_Attribute, () => (T) attribute);
    }

    public class Editor<T> : UnityEditor.Editor where T : Object {
        private T m_TypedTarget;
        private T[] m_TypedTargets;

        protected T Target => UnityUtils.GetOrSet(ref m_TypedTarget, () => target as T);
        protected T[] Targets => UnityUtils.GetOrSet(ref m_TypedTargets, () => targets.Cast<T>().ToArray());
    }
}