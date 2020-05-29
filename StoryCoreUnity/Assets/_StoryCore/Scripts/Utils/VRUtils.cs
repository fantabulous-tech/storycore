using UnityEngine;
using VRTK;
using System.Collections.Generic;

namespace StoryCore.Utils {
    public static class VRUtils {
        public static void Teleport(Transform t, bool recenterOnHeadset) {
            Transform playArea = VRTK_DeviceFinder.PlayAreaTransform();
            Transform hmd = recenterOnHeadset ? VRTK_DeviceFinder.HeadsetTransform() : null;
            Player.s_Instance.transform.position = t.position;
            List<Transform> children = playArea.GetChildren();
            bool[] priorState = new bool[children.Count];
            for (int i = 0; i < children.Count; i++) {
                bool b = children[i].gameObject.activeSelf;
                priorState[i] = b;
                if (b) {
                    children[i].gameObject.SetActive(false);
                }
            }
            playArea.transform.rotation = t.rotation;
            playArea.transform.position = t.position;
            for (int i = 0; i < children.Count; i++) {
                if (priorState[i]) {
                    children[i].gameObject.SetActive(true);
                }
            }

            if (hmd) {
                playArea.transform.position += (playArea.position - hmd.position).ZeroY();
            }
            if (Globals.RecenterTarget != null) {
                Globals.RecenterTarget.Value = t.gameObject;
            }
        }
    }
}