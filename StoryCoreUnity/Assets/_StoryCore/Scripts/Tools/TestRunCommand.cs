using UnityEngine;

namespace StoryCore {
    public class TestRunCommand : MonoBehaviour {
        [SerializeField] private string m_Command = "/scene office.test";
        [SerializeField] private bool m_Run;

        private void Update() {
            if (m_Run) {
                m_Run = false;
                Debug.Log("Running Command: " + m_Command);
                CommandManager.RunCommand(m_Command, OnCommandSuccess, OnCommandFail);
            }
        }

        private void OnCommandSuccess() {
            Debug.Log("Command Succeeded: " + m_Command, this);
        }

        private void OnCommandFail() {
            Debug.LogError("Command Failed: " + m_Command, this);
        }
    }
}