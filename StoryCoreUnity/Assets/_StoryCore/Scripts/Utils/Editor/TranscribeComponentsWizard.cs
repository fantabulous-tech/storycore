using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorStyles = UnityEditor.EditorStyles;
using Object = UnityEngine.Object;

namespace StoryCore {
    public class TranscribeComponentsWizard : ScriptableWizard {
        [SerializeField] private GameObject m_Source;
        [SerializeField] private GameObject m_Target;
        [SerializeField] private CopyGameObjectTask m_RootTask;
        private Vector2 m_Scroll;

        [MenuItem("Tools/Transcribe Components")]
        private static void CreateTranscribeComponentsWizard() {
            DisplayWizard<TranscribeComponentsWizard>("Transcribe Components", "Transcribe");
        }

        private void OnWizardUpdate() {
            if (m_RootTask == null) {
                m_Source = m_Source ? m_Source : Selection.activeGameObject;
                m_Target = m_Target ? m_Target : Selection.gameObjects.FirstOrDefault(go => go != m_Source);

                if (!AssetDatabase.GetAssetPath(m_Source).IsNullOrEmpty()) {
                    m_Source = null;
                }
                if (!AssetDatabase.GetAssetPath(m_Target).IsNullOrEmpty()) {
                    m_Target = null;
                }

                m_RootTask = new CopyGameObjectTask(m_Source, m_Source, m_Target, m_Target);
            }
        }

        private void OnWizardCreate() {
            m_RootTask.Copy();
            m_RootTask.MoveRelativeReferences();
            m_RootTask = new CopyGameObjectTask(m_RootTask.Source, m_RootTask.Source, m_RootTask.Target, m_RootTask.Target);
        }

        private void OnGUI() {
            if (m_RootTask == null) {
                OnWizardUpdate();
            }

            RowLayout layout = new RowLayout();
            GUI.Label(layout.SourceRect, "Source", EditorStyles.boldLabel);
            GUI.Label(layout.TargetRect, "Target", EditorStyles.boldLabel);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            m_RootTask.OnGUI();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            if (GUILayout.Button("Transcribe All")) {
                OnWizardCreate();
            }
        }

        private class CopyGameObjectTask {
            private static Type[] s_ExcludedComponents = {typeof(Transform), typeof(Animator), typeof(SkinnedMeshRenderer), typeof(MeshRenderer), typeof(MeshFilter)};

            private GameObject m_SourceRoot;
            private GameObject m_TargetRoot;
            private CopyComponentTask[] m_ComponentTasks;
            private CopyGameObjectTask[] m_SubTasks;
            private bool m_ImportantTask;

            public GameObject Source { get; private set; }
            public GameObject Target { get; private set; }

            public CopyGameObjectTask(GameObject sourceRoot, GameObject source, GameObject targetRoot, GameObject target) {
                Source = source;
                Target = target;
                m_SourceRoot = sourceRoot;
                m_TargetRoot = targetRoot;
                GetComponents();
                GetSubTasks();
            }

            private void GetComponents() {
                Component[] sourceComponents = Source.GetComponents<Component>().Where(NotExcluded).ToArray();
                Component[] targetComponents = Target ? Target.GetComponents<Component>().Where(NotExcluded).ToArray() : new Component[0];
                m_ComponentTasks = new CopyComponentTask[sourceComponents.Length];

                for (int i = 0; i < m_ComponentTasks.Length; i++) {
                    Component sourceComponent = sourceComponents[i];
                    Component targetComponent = targetComponents.FirstOrDefault(c => c && c.GetType() == sourceComponent.GetType());

                    if (targetComponent) {
                        targetComponents[targetComponents.IndexOf(targetComponent)] = null;
                    }

                    m_ComponentTasks[i] = new CopyComponentTask(sourceComponents[i], targetComponent, Target);
                }

                if (m_ComponentTasks.Any()) {
                    m_ImportantTask = true;
                }
            }

            private static bool NotExcluded(Component component) {
                return s_ExcludedComponents.All(ec => !ec.IsInstanceOfType(component));
            }

            private void GetSubTasks() {
                GameObject[] sourceChildren = Source.transform.GetChildren().Select(c => c.gameObject).ToArray();
                GameObject[] targetChildren = Target ? Target.transform.GetChildren().Select(c => c.gameObject).ToArray() : new GameObject[0];
                List<CopyGameObjectTask> copyTasks = new List<CopyGameObjectTask>();

                // Name match pass.
                for (int i = 0; i < sourceChildren.Length; i++) {
                    GameObject sourceChild = sourceChildren[i];
                    GameObject targetChild = targetChildren.FirstOrDefault(c => c && c.name.Equals(sourceChild.name, StringComparison.OrdinalIgnoreCase));

                    if (targetChild) {
                        copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, targetChild));
                        sourceChildren[i] = null;
                        targetChildren[targetChildren.IndexOf(targetChild)] = null;
                    }
                }

                for (int i = 0; i < sourceChildren.Length; i++) {
                    GameObject sourceChild = sourceChildren[i];

                    if (!sourceChild) {
                        continue;
                    }

                    GameObject targetChild = targetChildren.ElementAtOrDefault(i);

                    if (targetChild) {
                        copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, targetChild));
                        targetChildren[i] = null;
                        continue;
                    }

                    copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, null));
                }

                m_SubTasks = copyTasks.ToArray();
                m_ImportantTask = m_ImportantTask || m_SubTasks.Any(st => st.m_ImportantTask);
            }

            public void OnGUI(int indentLevel = 0) {
                if (!m_ImportantTask) {
                    return;
                }

                GUILayout.BeginHorizontal();
                RowLayout layout = new RowLayout(indentLevel);

                GameObject newSource = (GameObject) EditorGUI.ObjectField(layout.SourceRect, Source, typeof(GameObject), true);
                if (GUI.Button(layout.DividerRect, "->")) {
                    Copy();
                    MoveRelativeReferences();
                }
                GameObject newTarget = (GameObject) EditorGUI.ObjectField(layout.TargetRect, Target, typeof(GameObject), true);
                GUILayout.EndHorizontal();

                m_ComponentTasks.ForEach(ct => ct.OnGUI(indentLevel + 3));
                m_SubTasks.ForEach(st => st.OnGUI(indentLevel + 1));

                if (newSource != Source || newTarget != Target) {
                    if (Source == m_SourceRoot) {
                        m_SourceRoot = newSource;
                    }
                    if (Target == m_TargetRoot) {
                        m_TargetRoot = newTarget;
                    }
                    Source = newSource;
                    Target = newTarget;
                    GetComponents();
                    GetSubTasks();
                }
            }

            public void Copy() {
                if (!m_ImportantTask) {
                    return;
                }

                if (m_ComponentTasks.Any() && !Target) {
                    Debug.LogWarning($"Can't copy {Source.name} components. No matching target found.", Source);
                } else {
                    m_ComponentTasks.ForEach(t => t.Copy());
                }

                m_SubTasks.ForEach(t => t.Copy());
            }

            public void MoveRelativeReferences(string sourceRootPath = null, string targetRootPath = null) {
                if (!m_ImportantTask || !Target) {
                    return;
                }

                sourceRootPath = sourceRootPath ?? m_SourceRoot.FullName(FullName.Parts.FullScenePath);
                targetRootPath = targetRootPath ?? m_TargetRoot.FullName(FullName.Parts.FullScenePath);

                m_ComponentTasks.ForEach(t => t.MoveRelativeReferences(sourceRootPath, targetRootPath));
                m_SubTasks.ForEach(t => t.MoveRelativeReferences(sourceRootPath, targetRootPath));
            }
        }

        private class CopyComponentTask {
            private bool m_Enabled = true;
            private readonly Component m_SourceComponent;
            private Component m_TargetComponent;
            private readonly GameObject m_Target;

            public CopyComponentTask(Component sourceComponent, Component targetComponent, GameObject target) {
                m_SourceComponent = sourceComponent;
                m_TargetComponent = targetComponent;
                m_Target = target;
            }

            public void OnGUI(int indentLevel) {
                GUILayout.BeginHorizontal();
                RowLayout layout = new RowLayout(indentLevel, true);
                EditorGUI.ObjectField(layout.SourceRect, m_SourceComponent, typeof(Component), true);
                m_Enabled = GUI.Toggle(layout.ToggleRect, m_Enabled, GUIContent.none);
                if (GUI.Button(layout.DividerRect, "->")) {
                    Copy();
                }
                EditorGUI.ObjectField(layout.TargetRect, m_TargetComponent, typeof(Component), true);
                GUILayout.EndHorizontal();
            }

            public void Copy() {
                if (ComponentUtility.CopyComponent(m_SourceComponent)) {
                    if (m_TargetComponent) {
                        ComponentUtility.PasteComponentValues(m_TargetComponent);
                    } else if (ComponentUtility.PasteComponentAsNew(m_Target)) {
                        m_TargetComponent = m_Target.GetComponents<Component>().LastOrDefault();
                    }
                }
            }

            public void MoveRelativeReferences(string sourceRootPath, string targetRootPath) {
                if (!m_TargetComponent) {
                    return;
                }

                SerializedObject so = new SerializedObject(m_TargetComponent);
                SerializedProperty sp = so.GetIterator();

                while (sp.Next(true)) {
                    MoveRelativeReference(sourceRootPath, targetRootPath, sp);
                }

                so.ApplyModifiedProperties();
            }

            private void MoveRelativeReference(string sourceRootPath, string targetRootPath, SerializedProperty sp) {
                if (sp.propertyType != SerializedPropertyType.ObjectReference) {
                    return;
                }

                Object reference = sp.objectReferenceValue;

                if (!reference || !AssetDatabase.GetAssetPath(reference).IsNullOrEmpty()) {
                    return;
                }

                string scenePath = reference.FullName(FullName.Parts.FullScenePath);

                if (!scenePath.StartsWith(sourceRootPath, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                string newPath = scenePath.Replace(sourceRootPath, targetRootPath);
                Debug.Log(m_TargetComponent.GetType().Name + " has a " + reference.GetType().Name + " reference on " + m_TargetComponent.name + ": " + scenePath + " --> " + newPath, m_TargetComponent);
                GameObject targetObject = GameObject.Find(newPath);

                if (!targetObject) {
                    Debug.LogWarning("Couldn't find " + newPath);
                    return;
                }

                if (reference is GameObject) {
                    sp.objectReferenceValue = targetObject;
                } else {
                    sp.objectReferenceValue = targetObject.GetComponent(reference.GetType());

                    if (!sp.objectReferenceValue) {
                        Debug.LogWarning("Couldn't find a component that matches type " + reference.GetType().Name, reference);
                    }
                }
            }
        }

        private class RowLayout {
            private const float kCheckboxWidth = 15;
            private const float kDividerWidth = 25;
            private const float kIndent = 8;

            public readonly Rect ToggleRect;
            public readonly Rect SourceRect;
            public readonly Rect DividerRect;
            public readonly Rect TargetRect;

            public RowLayout(int indentLevel = 0, bool useCheckbox = false) {
                Rect editorRect = EditorGUILayout.GetControlRect(false);
                float indent = indentLevel*kIndent;
                float columnWidth = (editorRect.width - indent*2 - kDividerWidth)/2;
                float x = indent;

                ToggleRect = new Rect(editorRect) {x = x, width = useCheckbox ? kCheckboxWidth : 0};
                x += ToggleRect.width;
                SourceRect = new Rect(editorRect) {x = x, width = columnWidth - ToggleRect.width};
                x += SourceRect.width;
                DividerRect = new Rect(editorRect) {x = x, width = kDividerWidth};
                x += DividerRect.width + indent;
                TargetRect = new Rect(editorRect) {x = x, width = columnWidth};
            }
        }
    }
}