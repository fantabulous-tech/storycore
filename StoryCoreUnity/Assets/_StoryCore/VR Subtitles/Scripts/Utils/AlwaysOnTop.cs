using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRSubtitles.Utils {
    public class AlwaysOnTop : MonoBehaviour {
        public bool IncludeChildren = true;
        private static readonly int s_UnityGuizTestMode = Shader.PropertyToID("unity_GUIZTestMode");
        private DelaySequence m_UpdateRenderingModeDelay;

        private void OnEnable() {
            m_UpdateRenderingModeDelay = Delay.ForFrameCount(5, this).Then(UpdateRenderingMode);
        }

        private void OnDisable() {
            UpdateRenderingMode();
        }

        private void OnDestroy() {
            m_UpdateRenderingModeDelay?.Cancel("Object is being destroyed", this);
        }

        private void UpdateRenderingMode() {
            CanvasRenderer[] renderers = IncludeChildren ? GetComponentsInChildren<CanvasRenderer>() : GetComponents<CanvasRenderer>();
            renderers.ForEach(SetZTestMode);
        }

        private void SetZTestMode(CanvasRenderer r) {
            if (!r || r.materialCount == 0) {
                return;
            }

            Material oldMat = r.GetMaterial(0);

            if (!oldMat) {
                return;
            }

            Material newMat = new Material(oldMat);
            newMat.SetInt(s_UnityGuizTestMode, (int) (enabled ? CompareFunction.Always : CompareFunction.LessEqual));
            r.SetMaterial(newMat, 0);
        }
    }
}