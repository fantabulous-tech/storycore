using CoreUtils;
using CoreUtils.GameEvents;
using CoreUtils.GameVariables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VRTK;

namespace StoryCore.UI {
    public class Journal : MonoBehaviour {
        private enum JournalState {
            Unset,
            Closed,
            Open
        }

        [SerializeField] private Transform m_LeftSide;
        [SerializeField] private Transform m_RightSide;
        [SerializeField] private GameEvent m_OpenJournalEvent;
        [SerializeField] private GameEvent m_CloseJournalEvent;
        [SerializeField] private GameEvent m_ToggleMainMenu;
        [SerializeField] private StateMachine m_MenuStates;
        [SerializeField] private State m_MainMenuState;
        [SerializeField] private State m_ChoiceMenuState;
        [SerializeField] private float m_AutoCloseDistance = 2f;
        [SerializeField, AutoFillAsset] private ToggleMenuLocator m_ToggleMenuLocator;

        public UnityEvent PageSelected;
        private JournalState m_JournalState;
        private Transform m_Hmd;

        private Transform Hmd => UnityUtils.GetOrSet(ref m_Hmd, VRTK_DeviceFinder.HeadsetTransform);

        private void Awake() {
            m_OpenJournalEvent.GenericEvent += OnOpenJournal;
            m_CloseJournalEvent.GenericEvent += OnCloseJournal;
            m_ToggleMainMenu.GenericEvent += OnToggleMainMenu;
            SceneManager.sceneUnloaded += OnSceneLoaded;
            m_MenuStates.Exit();
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
            ShowPage(m_ChoiceMenuState);
            Physics.autoSimulation = false;
        }

        private void OnDisable() {
            m_LeftSide.localRotation = Quaternion.AngleAxis(90, Vector3.up);
            m_RightSide.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
            m_JournalState = JournalState.Closed;
            Physics.autoSimulation = true;
            m_MenuStates.Exit();
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
            if (gameObject.activeSelf && m_JournalState == JournalState.Open) {
                OnCloseJournal();
            } else {
                OnOpenJournal();
                ShowPage(m_MainMenuState);
            }
        }

        private void OnOpenJournal() {
            m_ToggleMenuLocator.Value.Open();
        }

        private void OnCloseJournal() {
            m_ToggleMenuLocator.Value.Close();
            m_JournalState = JournalState.Closed;
            m_MenuStates.Exit();
        }

        private void OnSceneLoaded(Scene scene) {
            m_ToggleMenuLocator.Value.Close();
            m_MenuStates.Exit();
            m_JournalState = JournalState.Closed;
        }

        private void ShowPage(State page) {
            m_JournalState = JournalState.Open;
            page.SetState();
            PageSelected.Invoke();
        }
    }
}