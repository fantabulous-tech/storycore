using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.Utils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class MovePoint : MonoBehaviour {
        [SerializeField] private VRTK_InteractableObject m_InteractableObject;
        [SerializeField] private GameObject m_Normal;
        [SerializeField] private GameObject m_Hover;
        [SerializeField] private GameObject m_Label;
        [SerializeField] private Collider m_Collision;

        private StoryTeller m_StoryTeller;
        private string m_MoveName;
        private readonly List<GameObject> m_Touchers = new List<GameObject>();
        private StoryChoice m_MoveChoice;

        public event Action Activate;
        public event Action Deactivate;

        private void Start() {
            if (!Globals.IsActive) {
                return;
            }

            m_StoryTeller = Globals.StoryTeller;
            m_StoryTeller.OnChoicesReady += UpdateState;
            m_StoryTeller.OnChoosing += OnChoosing;

            m_MoveName = name.Split(':').ElementAtOrDefault(1);

            UpdateState();

            m_InteractableObject.InteractableObjectTouched += OnTouch;
            m_InteractableObject.InteractableObjectUntouched += OnUntouch;
            m_InteractableObject.InteractableObjectUsed += OnUse;

            if (Globals.CommandRecenter != null) {
                Globals.CommandRecenter.GenericEvent += OnRecenter;
            }
        }

        private void OnDestroy() {
            if (!Globals.IsActive) {
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

            if (m_StoryTeller) {
                m_StoryTeller.OnChoicesReady -= UpdateState;
                m_StoryTeller.OnChoosing -= OnChoosing;
            }

            m_Touchers.Clear();
            UpdateState();

            if (Globals.CommandRecenter != null) {
                Globals.CommandRecenter.GenericEvent -= OnRecenter;
            }
        }

        private void OnChoosing(StoryChoice choice) {
            if (MyChoice(choice)) {
                Move();
            } else {
                UpdateState();
            }
        }

        private void Move() {
            Globals.RecenterTarget.Value = gameObject;
            Globals.CommandRecenter.Raise(name);
        }

        private void OnUse(object sender, InteractableObjectEventArgs e) {
            if (m_MoveChoice != null) {
                m_MoveChoice.Select();
            } else {
                Debug.LogWarningFormat(this, "No move choice found, but we are somehow trying to move to " + name);
            }
        }

        private void OnRecenter() {
            if (Globals.RecenterTarget.Value == gameObject) {
                VRUtils.Teleport(transform, true);
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
            return choice.IsValidChoice("move") && (m_MoveName.IsNullOrEmpty() || choice.Text.Contains(m_MoveName, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateState() {
            m_MoveChoice = m_StoryTeller.CurrentChoices.FirstOrDefault(MyChoice);
            bool isActive = m_MoveChoice != null;
            bool isHover = isActive && m_Touchers.Count > 0;

            if (!isActive) {
                m_Touchers.Clear();
            }

            bool wasActive = m_Collision.enabled;

            if (!wasActive && isActive) {
                Activate?.Invoke();
            } else if (wasActive && !isActive) {
                Deactivate?.Invoke();
            }

            m_Collision.enabled = isActive;

            m_Normal.SetActive(!isHover && isActive);
            m_Hover.SetActive(isHover);
            m_Label.SetActive(isHover);
        }
    }
}