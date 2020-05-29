using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRSubtitles.Utils {
    public class AlwaysOnTop : MonoBehaviour {
        public bool IncludeChildren = true;
        private static readonly int s_UnityGuizTestMode = Shader.PropertyToID("unity_GUIZTestMode");

        private void OnEnable() {
            Delay.ForFrameCount(5, this).Then(UpdateRenderingMode);
        }

        private void OnDisable() {
            UpdateRenderingMode();
        }

        private void UpdateRenderingMode() {
            CanvasRenderer[] renderers = IncludeChildren ? GetComponentsInChildren<CanvasRenderer>() : GetComponents<CanvasRenderer>();
            renderers.ForEach(SetZTestMode);
        }

        private void SetZTestMode(CanvasRenderer r) {
            if (r.materialCount == 0) {
                return;
            }
            Material mat = new Material(r.GetMaterial(0));
            mat.SetInt(s_UnityGuizTestMode, (int) (enabled ? CompareFunction.Always : CompareFunction.LessEqual));
            r.SetMaterial(mat, 0);
        }
    }
}