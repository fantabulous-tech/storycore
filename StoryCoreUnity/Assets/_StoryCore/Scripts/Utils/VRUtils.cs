using UnityEngine;
using VRTK;
using System.Collections.Generic;
using CoreUtils;

namespace StoryCore.Utils {
    public static class VRUtils {
        public static void Teleport(Transform t, bool recenterOnHeadset) {
            Transform playArea = VRTK_DeviceFinder.PlayAreaTransform();
            Transform hmd = recenterOnHeadset ? VRTK_DeviceFinder.HeadsetTransform() : null;
            playArea.position = t.position;
            List<Transform> children = playArea.GetChildren();
            bool[] priorState = new bool[children.Count];
            for (int i = 0; i < children.Count; i++) {
                bool b = children[i].gameObject.activeSelf;
                priorState[i] = b;
                if (b) {
                    children[i].gameObject.SetActive(false);
                }
            }
            playArea.rotation = t.rotation;
            playArea.position = t.position;
            for (int i = 0; i < children.Count; i++) {
                if (priorState[i]) {
                    children[i].gameObject.SetActive(true);
                }
            }

            if (hmd) {
                playArea.position += (playArea.position - hmd.position).ZeroY();
            }
            if (Globals.RecenterTarget != null) {
                Globals.RecenterTarget.Value = t.gameObject;
            }
        }
    }
}