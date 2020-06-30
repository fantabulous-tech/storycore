using System;
using StoryCore.GameEvents;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VRTK;

namespace StoryCore.UI {

    public class Journal : MonoBehaviour {
        public enum PageState {
            Unset,
            Closed,
            ChoiceMenu,
            MainMenu,
            SceneMenu
        }

        [Serializable]
        public class PageSet {
            public PageState PageState;
            public GameObject RightPage;
            public GameObject LeftPage;
        }

        [SerializeField] private Transform m_LeftSide;
        [SerializeField] private Transform m_RightSide;
        [SerializeField] private PageSet[] m_Pages;
        [SerializeField] private float m_AutoCloseDistance = 2f;

        //  --- Note: the GameVariableBool MenuOpen is controlled by a standalone component
        [SerializeField] private GameEvent m_CloseJournalEvent;
        [SerializeField] private GameEventPosAndRot m_MenuLocationMoveEvent;

        public UnityEvent PageSelected;
        private PageState m_PageState;
        private Transform m_Hmd;
        private int m_LastTooFarFrame;

        private Transform Hmd => UnityUtils.GetOrSet(ref m_Hmd, VRTK_DeviceFinder.HeadsetTransform);
        public PageSet[] Pages => m_Pages;

        private void Awake() {
            m_MenuLocationMoveEvent.Event += OnNewLocation;
            SceneManager.sceneUnloaded += OnSceneLoaded;
        }

        private void OnDestroy() {
            m_MenuLocationMoveEvent.Event -= OnNewLocation;
            SceneManager.sceneUnloaded -= OnSceneLoaded;
        }

        private void OnEnable() {
            m_LeftSide.localRotation = Quaternion.AngleAxis(90, Vector3.up);
            m_RightSide.localRotation = Quaternion.AngleAxis(-90, Vector3.up);

            //  --- Should probably chose based on which was last open.
            // ShowChoiceMenu();
            ShowMainMenu();

            Physics.autoSimulation = false;
        }

        private void OnDisable() {
            m_LeftSide.localRotation = Quaternion.AngleAxis(90, Vector3.up);
            m_RightSide.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
            m_PageState = PageState.Closed;
            Physics.autoSimulation = true;
        }

        private void Update() {
            // Simulate physics while the journal is open
            // so the colliders can get where they need to be
            // even when the game is paused.
            Physics.Simulate(Time.fixedUnscaledDeltaTime);

            CheckDistance();
        }

        private void OnNewLocation(PosAndRot placement) {
            transform.SetPositionAndRotation(placement.position, placement.rotation);
        }

        private void CheckDistance() {
            Transform hmd = Hmd;
            if (!hmd) {
                return;
            }

            float distanceSq = (transform.position - Hmd.position).sqrMagnitude;

            if (distanceSq > m_AutoCloseDistance*m_AutoCloseDistance) {
                //  --- Must be outside this distance for more than one frame,
                //      to give repositioners a chance.
                if (Time.frameCount - m_LastTooFarFrame == 1) {
                    Debug.Log("Turning journal off based on distance.");
                    m_CloseJournalEvent.Raise();
                }
                m_LastTooFarFrame = Time.frameCount;
            }
        }

        private void OnSceneLoaded(Scene scene) {
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

        public void ShowPage(PageState page) {
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