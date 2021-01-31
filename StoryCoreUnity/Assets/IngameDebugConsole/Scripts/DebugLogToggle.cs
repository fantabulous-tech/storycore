using System;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace IngameDebugConsole {
	public class DebugLogToggle : MonoBehaviour {
		[SerializeField] private KeyCode m_ToggleKey = KeyCode.Tab;
		[SerializeField] private DebugLogManager m_Manager;
		[SerializeField] private DebugLogPopup m_PopUp;
		[SerializeField] private PopupDisplay m_ShowPopup;

		private bool m_InitialDisplayComplete;

		private bool ShowPopup {
			get {
				switch (m_ShowPopup) {
					case PopupDisplay.Always:
						return true;
					case PopupDisplay.HideInitially:
						return m_InitialDisplayComplete;
					case PopupDisplay.HideInRelease:
						return Debug.isDebugBuild;
					case PopupDisplay.HideAlways:
						return false;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private enum PopupDisplay {
			Always,
			HideInitially,
			HideInRelease,
			HideAlways
		}

		private void Start() {
			m_Manager = m_Manager ? m_Manager : (m_Manager = GetComponent<DebugLogManager>());
			m_PopUp = m_PopUp ? m_PopUp : (m_PopUp = GetComponentInChildren<DebugLogPopup>());

			if (!ShowPopup) { Delay.OneFrame(this).Then(m_PopUp.Hide); }

			m_InitialDisplayComplete = true;
		}

		private void Update() {
			if (Input.GetKeyDown(m_ToggleKey)) { SetVisible(!m_Manager.IsLogWindowVisible); }
		}

		private void SetVisible(bool value) {
			if (value) {
				m_Manager.Show();
				m_PopUp.Hide();
				m_Manager.FocusInput();
			}
			else {
				m_Manager.Hide();

				if (ShowPopup) { m_PopUp.Show(); }
				else { m_PopUp.Hide(); }

				m_Manager.UnfocusInput();
				RemoveAddedChar();
			}
		}

		private void RemoveAddedChar() {
			char c1 = ToChar(m_ToggleKey);
			if (c1 == '\0') { return; }
			string text = m_Manager.CommandText;
			int pos = m_Manager.CommandCaret;
			char c2 = pos >= 1 ? text[pos - 1] : '\0';
			if (c1 == c2) { m_Manager.CommandText = text.Remove(pos - 1, 1); }
		}

		private static char ToChar(KeyCode code) {
			switch (code) {
				case KeyCode.Tab:
					return '\t';
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					return '\n';
				case KeyCode.Space:
					return ' ';
				case KeyCode.Alpha0:
				case KeyCode.Keypad0:
					return '0';
				case KeyCode.Alpha1:
				case KeyCode.Keypad1:
					return '1';
				case KeyCode.Alpha2:
				case KeyCode.Keypad2:
					return '2';
				case KeyCode.Alpha3:
				case KeyCode.Keypad3:
					return '3';
				case KeyCode.Alpha4:
				case KeyCode.Keypad4:
					return '4';
				case KeyCode.Alpha5:
				case KeyCode.Keypad5:
					return '5';
				case KeyCode.Alpha6:
				case KeyCode.Keypad6:
					return '6';
				case KeyCode.Alpha7:
				case KeyCode.Keypad7:
					return '7';
				case KeyCode.Alpha8:
				case KeyCode.Keypad8:
					return '8';
				case KeyCode.Alpha9:
				case KeyCode.Keypad9:
					return '9';
				case KeyCode.Period:
				case KeyCode.KeypadPeriod:
					return '.';
				case KeyCode.Slash:
				case KeyCode.KeypadDivide:
					return '/';
				case KeyCode.Asterisk:
				case KeyCode.KeypadMultiply:
					return '*';
				case KeyCode.Minus:
				case KeyCode.KeypadMinus:
					return '-';
				case KeyCode.Plus:
				case KeyCode.KeypadPlus:
					return '+';
				case KeyCode.Equals:
				case KeyCode.KeypadEquals:
					return '=';
				case KeyCode.Exclaim:
					return '!';
				case KeyCode.DoubleQuote:
					return '"';
				case KeyCode.Hash:
					return '#';
				case KeyCode.Dollar:
					return '$';
				case KeyCode.Ampersand:
					return '&';
				case KeyCode.Quote:
					return '\'';
				case KeyCode.LeftParen:
					return '(';
				case KeyCode.RightParen:
					return ')';
				case KeyCode.Comma:
					return ',';
				case KeyCode.Colon:
					return ':';
				case KeyCode.Semicolon:
					return ';';
				case KeyCode.Less:
					return '<';
				case KeyCode.Greater:
					return '?';
				case KeyCode.Question:
					return '?';
				case KeyCode.At:
					return '@';
				case KeyCode.LeftBracket:
					return '[';
				case KeyCode.Backslash:
					return '\\';
				case KeyCode.RightBracket:
					return ']';
				case KeyCode.Caret:
					return '^';
				case KeyCode.Underscore:
					return '_';
				case KeyCode.BackQuote:
					return '`';
				case KeyCode.A:
				case KeyCode.B:
				case KeyCode.C:
				case KeyCode.D:
				case KeyCode.E:
				case KeyCode.F:
				case KeyCode.G:
				case KeyCode.H:
				case KeyCode.I:
				case KeyCode.J:
				case KeyCode.K:
				case KeyCode.L:
				case KeyCode.M:
				case KeyCode.N:
				case KeyCode.O:
				case KeyCode.P:
				case KeyCode.Q:
				case KeyCode.R:
				case KeyCode.S:
				case KeyCode.T:
				case KeyCode.U:
				case KeyCode.V:
				case KeyCode.W:
				case KeyCode.X:
				case KeyCode.Y:
				case KeyCode.Z:
					return code.ToString().ToLower()[0];
				default:
					return '\0';
			}
		}
	}
}