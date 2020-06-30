// ---------------------------------------------------------------------
// ---------------------------------------------------------------------

#pragma warning disable 0649 //Disable the "...  is never assigned to, and will always have its default value" warning

using StoryCore.GameEvents;
using UnityEngine;

namespace StoryCore.UI {
    /*
     |  --- JournalOpener
     |
     | Author: Dan Kyles 18/06/2020
     */
    public class JournalOpener : MonoBehaviour {

        [SerializeField] private Journal m_JournalRoot;

        //  --- Note: the GameVariableBool MenuOpen is controlled by a standalone component
        [SerializeField] private GameEvent m_OpenJournalEvent;
        [SerializeField] private GameEvent m_CloseJournalEvent;
        [SerializeField] private GameEvent m_ToggleMainMenu;

        private void Awake() {
            m_OpenJournalEvent.GenericEvent += OnToggleChoiceMenu;
            m_CloseJournalEvent.GenericEvent += OnCloseJournal;
            m_ToggleMainMenu.GenericEvent += OnToggleMainMenu;
            m_JournalRoot.gameObject.SetActive(false);
        }

        private void OnDestroy() {
            m_OpenJournalEvent.GenericEvent -= OnToggleChoiceMenu;
            m_CloseJournalEvent.GenericEvent -= OnCloseJournal;
            m_ToggleMainMenu.GenericEvent -= OnToggleMainMenu;
        }

        private void OpenMenu() {
            m_JournalRoot.gameObject.SetActive(true);
            m_JournalRoot.ShowMainMenu();
        }

        public void CloseMenu() {
            m_CloseJournalEvent.Raise();
        }

        private void OnToggleMainMenu() {
            if (m_JournalRoot.gameObject.activeSelf) {
                CloseMenu();
            } else {
                OpenMenu();
            }
        }

        private void OnToggleChoiceMenu() {
            if (m_JournalRoot.gameObject.activeSelf) {
                CloseMenu();
            } else {
                m_JournalRoot.gameObject.SetActive(true);
                m_JournalRoot.ShowChoiceMenu();
            }
        }

        private void OnCloseJournal() {
            m_JournalRoot.gameObject.SetActive(false);
        }
    }
}