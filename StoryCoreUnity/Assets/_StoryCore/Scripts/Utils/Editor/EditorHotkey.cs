using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StoryCore.Utils {
    public class EditorHotKey {
        private const string kKeyPref = "Key";
        private const string kAltPref = "Alt";
        private const string kCtrlPref = "Ctrl";
        private const string kShiftPref = "Shift";
        private const string kEnabledPref = "Enabled";

        private static GUIStyle s_ButtonOff;
        private static GUIStyle s_ButtonOn;
        private static GUIStyle s_Popup;

        private string Name { get; set; }
        private bool Enabled { get; set; }
        private Action Action { get; set; }
        private KeyCode KeyCode { get; set; }
        private bool Alt { get; set; }
        private bool Ctrl { get; set; }
        private bool Shift { get; set; }

        public EditorHotKey(string name, Action action, KeyCode defaultKey, bool defaultCtrl = false, bool defaultAlt = false, bool defaultShift = false) {
            Name = name;
            Enabled = EditorPrefs.GetBool(Name + kEnabledPref, true);
            Ctrl = EditorPrefs.GetBool(Name + kCtrlPref, defaultCtrl);
            Alt = EditorPrefs.GetBool(Name + kAltPref, defaultAlt);
            Shift = EditorPrefs.GetBool(Name + kShiftPref, defaultShift);
            KeyCode = (KeyCode) EditorPrefs.GetInt(Name + kKeyPref, (int) defaultKey);
            Action = action;
        }

        private bool Pressed {
            get {
                if (!Enabled) {
                    return false;
                }
                if (Event.current == null) {
                    return false;
                }
                if (Event.current.type != EventType.KeyUp) {
                    return false;
                }
                return Event.current.keyCode == KeyCode && Event.current.alt == Alt && Event.current.control == Ctrl && Event.current.shift == Shift;
            }
        }

        public void OnGUI() {
            if (s_ButtonOff == null) {
                s_ButtonOff = new GUIStyle(GUI.skin.GetStyle("Button")) {fixedWidth = 70};
                s_ButtonOn = new GUIStyle(s_ButtonOff) {normal = {textColor = Color.white}};
                s_ButtonOn.normal.background = s_ButtonOn.active.background;
                s_ButtonOff.normal.textColor = new Color(0.45f, 0.45f, 0.45f);
                s_Popup = new GUIStyle(GUI.skin.GetStyle("Popup")) {fixedHeight = 18, normal = {textColor = Color.white}};
            }

            GUILayout.BeginHorizontal();

            Enabled = GUILayout.Toggle(Enabled, " " + Name, GUILayout.Width(150));

            GUI.enabled = Enabled;

            if (GUILayout.Button("Ctrl", Ctrl ? s_ButtonOn : s_ButtonOff)) {
                Ctrl = !Ctrl;
            }
            if (GUILayout.Button("Alt", Alt ? s_ButtonOn : s_ButtonOff)) {
                Alt = !Alt;
            }
            if (GUILayout.Button("Shift", Shift ? s_ButtonOn : s_ButtonOff)) {
                Shift = !Shift;
            }

            KeyCode = (KeyCode) EditorGUILayout.EnumPopup(KeyCode, s_Popup);
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            if (GUI.changed) {
                OnGUIChanged();
            }
        }

        private void OnGUIChanged() {
            EditorPrefs.SetBool(Name + kEnabledPref, Enabled);
            EditorPrefs.SetBool(Name + kCtrlPref, Ctrl);
            EditorPrefs.SetBool(Name + kAltPref, Alt);
            EditorPrefs.SetBool(Name + kShiftPref, Shift);
            EditorPrefs.SetInt(Name + kKeyPref, (int) KeyCode);
        }

        public void OnEditorUpdate() {
            if (Pressed) {
                Action?.Invoke();
            }
        }

        public void OnPlaymode() {
            Keyboard.OnKeyDown(Name, GetKeyCombo(), Action);
        }

        private KeyCode[] GetKeyCombo() {
            List<KeyCode> combo = new List<KeyCode> {KeyCode};
            if (Alt) {
                combo.Add(KeyCode.LeftAlt);
            }
            if (Ctrl) {
                combo.Add(KeyCode.LeftControl);
            }
            if (Shift) {
                combo.Add(KeyCode.LeftShift);
            }
            return combo.ToArray();
        }
    }
}