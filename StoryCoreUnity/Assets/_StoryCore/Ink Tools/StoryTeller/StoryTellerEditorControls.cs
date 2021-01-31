using System.Collections.Generic;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore {
    public class StoryTellerEditorControls : MonoBehaviour {
        [SerializeField] private StoryTeller m_StoryTeller;
        [SerializeField] private Vector2 m_Offset = new Vector2(10, 10);
        [SerializeField] private float m_Width = 300;
        [SerializeField] private float m_ChoiceHeight = 20;
        [SerializeField, Range(0, 1)] private float m_BackgroundAlpha = 0.4f;

        private static GUIStyle s_BoxStyle;
        private static Texture2D s_BackgroundTexture;

        private GUIStyle BoxStyle => UnityUtils.GetOrSet(ref s_BoxStyle, CreateBoxStyle);

        private void Start() {
            if (!Application.isEditor) {
                enabled = false;
            }
        }

        private void Update() {

            int keyDownIdx = -1;
            if (!Input.anyKeyDown) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) { keyDownIdx = 0; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { keyDownIdx = 1; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { keyDownIdx = 2; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { keyDownIdx = 3; }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { keyDownIdx = 4; }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { keyDownIdx = 5; }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { keyDownIdx = 6; }
            if (Input.GetKeyDown(KeyCode.Alpha8)) { keyDownIdx = 7; }
            if (Input.GetKeyDown(KeyCode.Alpha9)) { keyDownIdx = 8; }
            if (Input.GetKeyDown(KeyCode.Alpha0)) { keyDownIdx = 9; }

            if (keyDownIdx < 0) {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift)) {
                keyDownIdx += 10;
            }
            if (Input.GetKey(KeyCode.LeftControl)) {
                keyDownIdx += 10;
            }

            List<StoryChoice> currentChoices = m_StoryTeller.CurrentChoices;

            if (keyDownIdx < currentChoices.Count) {
                currentChoices[keyDownIdx].Choose();
            }
        }

        private void OnGUI() {
            if (m_StoryTeller == null || m_StoryTeller.Story == null) {
                return;
            }

            int choiceCount = m_StoryTeller.CurrentChoices.Count;
            float totalHeight = choiceCount*m_ChoiceHeight;

            if (choiceCount == 0) {
                return;
            }

            GUI.Box(new Rect(m_Offset.x, m_Offset.y, m_Width + 20, totalHeight + 20), GUIContent.none, BoxStyle);

            for (int i = 0; i < choiceCount; i++) {
                StoryChoice choice = m_StoryTeller.CurrentChoices[i];
                GUI.Label(new Rect(m_Offset.x + 10.0f, m_Offset.y + 10.0f + i*m_ChoiceHeight, m_Width, m_ChoiceHeight), $"{i + 1}: {choice.Key}");
            }
        }

        private void OnValidate() {
            s_BoxStyle = null;
        }

        private GUIStyle CreateBoxStyle() {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            s_BackgroundTexture = new Texture2D(2, 2);
            Color[] pix = new Color[2*2];

            for (int i = 0; i < pix.Length; i++) {
                pix[i] = new Color(0, 0, 0, m_BackgroundAlpha);
            }

            s_BackgroundTexture.SetPixels(pix);
            s_BackgroundTexture.Apply();
            boxStyle.normal.background = s_BackgroundTexture;
            return boxStyle;
        }
    }
}