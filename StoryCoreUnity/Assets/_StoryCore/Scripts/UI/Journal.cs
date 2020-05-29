using System;
using StoryCore.GameEvents;
using StoryCore.GameVariables;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VRTK;

namespace StoryCore.UI {
    public class Journal : MonoBehaviour {
        private enum PageState {
            Unset,
            Closed,
            ChoiceMenu,
            MainMenu,
            SceneMenu
        }

        [Serializable]
        private class PageSet {
            public PageState PageState;
            public GameObject RightPage;
            public GameObject LeftPage;
        }

        [SerializeField] private Transform m_LeftSide;
        [SerializeField] private Transform m_RightSide;
        [SerializeField] private PageSet[] m_Pages;
        [SerializeField] private GameEvent m_OpenJournalEvent;
        [SerializeField] private GameEvent m_CloseJournalEvent;
        [SerializeField] private GameEvent m_ToggleMainMenu;
        [SerializeField] private float m_AutoCloseDistance = 2f;
        [SerializeField] private ToggleMenuLocator m_ToggleMenuLocator;

        public UnityEvent PageSelected;
        private PageState m_PageState;
        private Transform m_Hmd;

        private Transform Hmd => UnityUtils.GetOrSet(ref m_Hmd, VRTK_DeviceFinder.HeadsetTransform);

        private void Awake() {
            m_OpenJournalEvent.GenericEvent += OnOpenJournal;
            m_CloseJournalEvent.GenericEvent += OnCloseJournal;
            m_ToggleMainMenu.GenericEvent += OnToggleMainMenu;
            SceneManager.sceneUnloaded += OnSceneLoaded;
            gameObject.SetActive(false);
        }

        private void OnDestroy() {
            m_OpenJournalEvent.GenericEvent -= OnOpenJournal;
            m_CloseJournalEvent.GenericEvent -= OnCloseJournal;
            m_ToggleMainMenu.GenericEvent -= OnToggleMainMenu;
            SceneManager.sceneUnloaded -= OnSceneLoaded;
        }

        private void OnEnable() {
            m_LeftSide.localRotation = Quaternion.AngleAxis(90, Vector3.up);
            m_RightSide.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
            ShowChoiceMenu();
            Physics.autoSimulation = false;
        }

        private void OnDisable() {
            m_LeftSide.localRotation = Quaternion.AngleAxis(90, Vector3.up);
            m_RightSide.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
            m_PageState = PageState.Closed;
            Physics.autoSimulation = true;
        }

        private void Update() {
            CheckDistance();

            // Simulate physics while the journal is open
            // so the colliders can get where they need to be
            // even when the game is paused.
            Physics.Simulate(Time.fixedUnscaledDeltaTime);
        }

        private void CheckDistance() {
            if (transform.position.DistanceTo(Hmd.position) > m_AutoCloseDistance) {
                Debug.Log("Turning journal off based on distance.");
                m_CloseJournalEvent.Raise();
            }
        }

        private void OnToggleMainMenu() {
            if (gameObject.activeSelf && m_PageState == PageState.MainMenu) {
                OnCloseJournal();
            } else {
                OnOpenJournal();
                ShowMainMenu();
            }
        }

        private void OnOpenJournal() {
            m_ToggleMenuLocator.Value.Open();
        }

        private void OnCloseJournal() {
            m_ToggleMenuLocator.Value.Close();
            m_PageState = PageState.Closed;
        }

        private void OnSceneLoaded(Scene scene) {
            m_ToggleMenuLocator.Value.Close();
            m_PageState = PageState.Closed;
        }

        public void ShowMainMenu() {
            ShowPage(PageState.MainMenu);
        }

        public void ShowChoiceMenu() {
            ShowPage(PageState.ChoiceMenu);
        }

        public void ShowSceneMenu() {
            ShowPage(PageState.SceneMenu);
        }

        private void ShowPage(PageState page) {
            m_PageState = page;
            UpdatePages();
            PageSelected.Invoke();
        }

        private void UpdatePages() {
            foreach (PageSet pageSet in m_Pages) {
                pageSet.RightPage.SetActive(pageSet.PageState == m_PageState);
                pageSet.LeftPage.SetActive(pageSet.PageState == m_PageState);
            }
        }
    }
}