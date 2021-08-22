using System;
using System.Collections.Generic;
using System.Linq;
using CoreUtils;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class MovePoint : MonoBehaviour {
        [SerializeField] private VRTK_InteractableObject m_InteractableObject;
        [SerializeField] private GameObject m_Inactive;
        [SerializeField] private GameObject m_Normal;
        [SerializeField] private GameObject m_Hover;
        [SerializeField] private GameObject m_Label;
        [SerializeField] private Collider m_Collision;
        [SerializeField] private bool m_IsChoice = true;
        [SerializeField] private bool m_AlwaysHover;
        [SerializeField] private bool m_RecenterOnHeadset = true;
        [SerializeField] private bool m_MatchRotation = true;

        private StoryTeller m_StoryTeller;
        private string m_MoveName;
        private readonly List<GameObject> m_Touchers = new List<GameObject>();
        private StoryChoice m_MoveChoice;
        private MoveStates m_MoveState;

        private bool IsActive => m_MoveState != MoveStates.Inactive;

        public enum MoveStates {
            Inactive,
            Normal,
            Hover
        }

        public event Action Activate;
        public event Action Deactivate;
        public event Action<MoveStates> StateChange;

        private void Start() {
            if (!Globals.Exists) {
                return;
            }

            m_InteractableObject.InteractableObjectTouched += OnTouch;
            m_InteractableObject.InteractableObjectUntouched += OnUntouch;
            m_InteractableObject.InteractableObjectUsed += OnUse;

            if (m_IsChoice) {
                m_StoryTeller = Globals.StoryTeller;
                m_StoryTeller.OnChoicesReady += UpdateState;
                m_StoryTeller.OnChoosing += OnChoosing;
                m_StoryTeller.OnChosen += UpdateState;
                m_MoveName = name.Split(':').ElementAtOrDefault(1);
            }

            if (Globals.CommandRecenter != null) {
                Globals.CommandRecenter.GenericEvent += OnRecenter;
            }

            if (Globals.RecenterTarget != null) {
                Globals.RecenterTarget.Changed += OnRecenterTargetChanged;
            }

            UpdateState();
        }

        private void OnRecenterTargetChanged(GameObject obj) {
            UpdateState();
        }

        private void OnTriggerEnter(Collider other) {
            VRTK_PlayerObject playerObject = other.GetComponent<VRTK_PlayerObject>();

            if (playerObject && playerObject.objectType == VRTK_PlayerObject.ObjectTypes.Headset) {
                StoryDebug.Log($"Player headset entered MovePoint {name}. Triggering move.", this);
                OnUse(this, new InteractableObjectEventArgs {interactingObject = other.gameObject});
            }
        }

        private void OnDestroy() {
            if (!Globals.Exists) {
                return;
            }

            if (AppTracker.IsQuitting) {
                return;
            }

            if (m_InteractableObject) {
                m_InteractableObject.InteractableObjectTouched -= OnTouch;
                m_InteractableObject.InteractableObjectUntouched -= OnUntouch;
                m_InteractableObject.InteractableObjectUsed -= OnUse;
            }

            if (m_IsChoice && m_StoryTeller) {
                m_StoryTeller.OnChoicesReady -= UpdateState;
                m_StoryTeller.OnChoosing -= OnChoosing;
                m_StoryTeller.OnChosen -= UpdateState;
            }

            if (Globals.CommandRecenter != null) {
                Globals.CommandRecenter.GenericEvent -= OnRecenter;
            }

            if (Globals.RecenterTarget != null) {
                Globals.RecenterTarget.Changed -= OnRecenterTargetChanged;
            }

            m_Touchers.Clear();
            UpdateState();
        }

        private void OnChoosing(StoryChoice choice) {
            if (MyChoice(choice)) {
                Move();
            }
            UpdateState();
        }

        private void Move() {
            Globals.RecenterTarget.Value = gameObject;
            Globals.CommandRecenter.Raise(name);
        }

        private void OnUse(object sender, InteractableObjectEventArgs e) {
            if (!m_IsChoice) {
                VRUtils.Teleport(transform, m_RecenterOnHeadset, m_MatchRotation);
                Globals.RecenterTarget.Value = gameObject;
            } else if (m_MoveChoice != null) {
                m_MoveChoice.Choose();
            } else {
                Debug.LogWarningFormat(this, "No move choice found, but we are somehow trying to move to " + name);
            }
        }

        private void OnRecenter() {
            if (Globals.RecenterTarget.Value == gameObject) {
                VRUtils.Teleport(transform, m_RecenterOnHeadset, m_MatchRotation);
            }
        }

        private void OnTouch(object sender, InteractableObjectEventArgs e) {
            m_Touchers.Add(e.interactingObject);
            UpdateState();
        }

        private void OnUntouch(object sender, InteractableObjectEventArgs e) {
            m_Touchers.Remove(e.interactingObject);
            UpdateState();
        }

        private bool MyChoice(StoryChoice choice) {
            return choice.IsValidChoice("move") && (m_MoveName.IsNullOrEmpty() || choice.Key.Contains(m_MoveName, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateState() {
            if (m_IsChoice) {
                m_MoveChoice = m_StoryTeller.CurrentChoices.FirstOrDefault(MyChoice);

                if (m_MoveChoice == null) {
                    m_MoveState = MoveStates.Inactive;
                } else if (m_Touchers.Count == 0 && !m_AlwaysHover) {
                    m_MoveState = MoveStates.Normal;
                } else {
                    m_MoveState = MoveStates.Hover;
                }
            } else {
                if (Globals.RecenterTarget.Value == gameObject) {
                    m_MoveState = MoveStates.Inactive;
                } else if (m_Touchers.Count == 0) {
                    m_MoveState = MoveStates.Normal;
                } else {
                    m_MoveState = MoveStates.Hover;
                }
            }

            if (!IsActive) {
                m_Touchers.Clear();
            }

            bool wasActive = m_Collision.enabled;

            if (!wasActive && IsActive) {
                Activate?.Invoke();
            } else if (wasActive && !IsActive) {
                Deactivate?.Invoke();
            }

            m_Collision.enabled = IsActive;


            switch (m_MoveState) {
                case MoveStates.Inactive:
                    SetActive(m_Normal, false);
                    SetActive(m_Hover, false);
                    SetActive(m_Label, false);
                    SetActive(m_Inactive, true);
                    break;
                case MoveStates.Normal:
                    SetActive(m_Inactive, false);
                    SetActive(m_Hover, false);
                    SetActive(m_Label, false);
                    SetActive(m_Normal, true);
                    break;
                case MoveStates.Hover:
                    SetActive(m_Inactive, false);
                    SetActive(m_Normal, false);
                    SetActive(m_Hover, true);
                    SetActive(m_Label, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StateChange?.Invoke(m_MoveState);
            // StoryDebug.Log($"Move point {name} updated state to {m_MoveState}");
        }

        private void SetActive(GameObject obj, bool state) {
            if (obj) {
                obj.SetActive(state);
            }
        }
    }
}