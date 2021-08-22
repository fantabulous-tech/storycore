using System;
using CoreUtils;
using UnityEngine;

namespace StoryCore {
    public class MovePointMaterials : MonoBehaviour {
        [SerializeField, AutoFillFromParent] private MovePoint m_MovePoint;
        [SerializeField, AutoFill] private Renderer m_Renderer;
        [SerializeField] private int m_MaterialIndex;

        [SerializeField] private Material m_InactiveMaterial;
        [SerializeField] private Material m_NormalMaterial;
        [SerializeField] private Material m_HoverMaterial;

        private void OnEnable() {
            m_MovePoint.StateChange += OnStateChange;
        }

        private void OnDisable() {
            if (m_MovePoint != null) {
                m_MovePoint.StateChange -= OnStateChange;
            }
        }

        private void OnStateChange(MovePoint.MoveStates state) {
            switch (state) {
                case MovePoint.MoveStates.Inactive:
                    SetMaterial(m_InactiveMaterial);
                    break;
                case MovePoint.MoveStates.Normal:
                    SetMaterial(m_NormalMaterial);
                    break;
                case MovePoint.MoveStates.Hover:
                    SetMaterial(m_HoverMaterial);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void SetMaterial(Material mat) {
            Material[] mats = m_Renderer.materials;
            mats[m_MaterialIndex] = mat;
            m_Renderer.materials = mats;
        }
    }
}