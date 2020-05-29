using System;
using System.Linq;
using DG.Tweening;
using RogoDigital;
using RogoDigital.Lipsync;
using RootMotion.FinalIK;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VRTK;
using Object = UnityEngine.Object;

namespace StoryCore.Commands {
    public class Character : BaseCharacter, IPerformAnim, IPerformLipSync, IPerformPlayable, IPerformAudio {
        [SerializeField] private AudioSource m_VoiceSource;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private LipSync m_LipSync;
        [SerializeField] private EyeController m_EyeController;
        [SerializeField] private LookAtIK m_LookAtIK;
        [SerializeField] private AnimationClip m_IdleAnim;

        private const float kLookAtTransition = 0.5f;

        private PlayableGraph m_Graph;
        private AnimationLayerMixerPlayable m_LayerMixer;
        private AnimationClipPlayable m_LastAnim;
        private Sequence m_Sequence;
        private AnimationMixerPlayable m_TransitionMixer;
        private AnimationClip m_LastIdleAnim;
        private Transform m_LeftFoot;
        private Transform m_RightFoot;
        private Transform m_LeftToes;
        private Transform m_RightToes;

        private Transform Target {
            get {
                if (m_LookAtIK) {
                    return m_LookAtIK.solver.target;
                }

                return m_EyeController ? m_EyeController.viewTarget : null;
            }
        }

        public override Transform AttentionPoint => m_EyeController && m_EyeController.LeftEyeLookAtBone ? m_EyeController.LeftEyeLookAtBone : m_VoiceSource ? m_VoiceSource.transform : transform;

        public AnimationClip CurrentAnim => m_LastAnim.IsValid() ? m_LastAnim.GetAnimationClip() : null;

        protected override void Awake() {
            base.Awake();
            m_VoiceSource = m_VoiceSource ? m_VoiceSource : GetComponentInChildren<AudioSource>();
            m_Animator = m_Animator ? m_Animator : GetComponent<Animator>();
            m_LipSync = m_LipSync ? m_LipSync : GetComponent<LipSync>();
            m_EyeController = m_EyeController ? m_EyeController : GetComponent<EyeController>();
            SetEmotion(kNeutral, 1);
        }

        private void OnEnable() {
            if (!m_Animator) {
                Debug.LogFormat(this, "Note: No Animator found for {0}", name);
                return;
            }

            SetupAnimator();
            InitTarget();

            if (VRTK_SDKManager.instance != null) {
                VRTK_SDKManager.instance.LoadedSetupChanged += OnSetupChanged;
            }
        }

        private void Update() {
            CheckIdle();
            CheckEmotions();
        }

        private void CheckIdle() {
            if (m_IdleAnim != m_LastIdleAnim) {
                m_LastIdleAnim = m_IdleAnim;
                PlayAnim(m_IdleAnim);
            }
        }

        private void OnDisable() {
            if (m_Graph.IsValid()) {
                m_Graph.Destroy();
            }
            if (VRTK_SDKManager.instance != null) {
                VRTK_SDKManager.instance.LoadedSetupChanged -= OnSetupChanged;
            }
        }

        private static AnimationClipPlayable GetClipPlayable(PlayableGraph graph, AnimationClip clip) {
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            playable.SetApplyFootIK(true);
            playable.SetApplyPlayableIK(true);
            return playable;
        }

        private void SetupAnimator() {
            m_Graph = PlayableGraph.Create(name + " Character Anims");
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            // Create the starting animation clip.
            m_LastAnim = GetClipPlayable(m_Graph, m_IdleAnim);

            m_LastIdleAnim = m_IdleAnim;

            // Create the transition mixer for changing animations over time.
            m_TransitionMixer = AnimationMixerPlayable.Create(m_Graph, 2);

            // Connect the base clip to the transition mixer.
            m_Graph.Connect(m_LastAnim, 0, m_TransitionMixer, 1);

            m_TransitionMixer.SetInputWeight(0, 0);
            m_TransitionMixer.SetInputWeight(1, 1);

            // Create the layer output to handle 'heels'/'barefoot' options.
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(m_Graph, "LayerMixer", m_Animator);
            m_LayerMixer = AnimationLayerMixerPlayable.Create(m_Graph, 2);

            // Set the 'heels' layer to additive.
            m_LayerMixer.SetLayerAdditive(1, true);
            playableOutput.SetSourcePlayable(m_LayerMixer);

            m_LayerMixer.ConnectInput(0, m_TransitionMixer, 0, 1);

            m_Graph.Play();
        }

        private void OnSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            if (e.currentSetup != null) {
                InitTarget();
            }
        }

        private void InitTarget() {
            if (!Target || !Target.gameObject.activeInHierarchy) {
                LookAt(UnityUtils.CameraTransform);
            }
        }

        public override void LookAt(Transform target) {
            Debug.Log($"{Name} now looking at {target}", this);
            if (m_LookAtIK != null) {
                m_LookAtIK.DOKill();
                Sequence sequence = DOTween.Sequence();
                sequence.Append(DOTween.To(m_LookAtIK.solver.GetIKPositionWeight, m_LookAtIK.solver.SetIKPositionWeight, 0f, kLookAtTransition));
                sequence.AppendCallback(() => m_LookAtIK.solver.target = target);
                sequence.Append(DOTween.To(m_LookAtIK.solver.GetIKPositionWeight, m_LookAtIK.solver.SetIKPositionWeight, 1f, kLookAtTransition));
                sequence.SetTarget(m_LookAtIK);
            }
            if (m_EyeController != null) {
                m_EyeController.viewTarget = target;
            }
        }

        public override DelaySequence SetEmotion(ScriptCommandInfo command) {
            string emotionName = command.Params.GetFirst();
            int emotionIndex = m_LipSync.emotions.IndexOf(e => Equals(e.emotion, emotionName));

            if (emotionIndex < 0) {
                Debug.LogWarning($"Could not find emotion '{emotionName}' on character '{name}'.", this);
                SetEmotion(kNeutral, 1);
                return DelaySequence.Empty;
            }

            if (command.Params.Length > 1 && int.TryParse(command.Params[1], out int intensity)) {
                SetEmotion(emotionName, Mathf.Clamp01(intensity/100f));
            } else {
                SetEmotion(emotionName, 1);
            }

            return DelaySequence.Empty;
        }

        private bool m_ChangeEmotions;

        private string m_LastEmotion;
        private int m_LastEmotionIndex;
        private float m_LastEmotionStart;

        private string m_CurrentEmotion;
        private int m_CurrentEmotionIndex;
        private float m_CurrentEmotionStart;
        private float m_CurrentEmotionProgress;
        private float m_CurrentEmotionTarget;

        private float m_TargetTime;
        private const float kEmotionTransition = 0.3f;
        private const string kNeutral = "Neutral";

        private void CheckEmotions() {
            if (!m_ChangeEmotions) {
                return;
            }

            float progress = 1 - Mathf.Clamp01((m_TargetTime - Time.time)/kEmotionTransition);

            if (progress >= 1) {
                m_ChangeEmotions = false;
                UpdateEmotions(1);
            } else {
                UpdateEmotions(progress);
            }
        }

        public void SetEmotion(string emotionName, float intensity) {
            m_ChangeEmotions = true;

            bool isSame = !m_LastEmotion.IsNullOrEmpty() && Equals(m_LastEmotion, m_CurrentEmotion);

            m_LastEmotion = m_CurrentEmotion;
            m_LastEmotionIndex = m_CurrentEmotionIndex;
            m_LastEmotionStart = Mathf.Lerp(m_CurrentEmotionStart, m_CurrentEmotionTarget, m_CurrentEmotionProgress);

            m_CurrentEmotion = emotionName;
            m_CurrentEmotionIndex = m_LipSync.emotions.IndexOf(e => Equals(e.emotion, emotionName));
            m_CurrentEmotionStart = isSame ? m_CurrentEmotionProgress : 0;
            m_CurrentEmotionProgress = 0;
            m_CurrentEmotionTarget = m_CurrentEmotion.Equals(kNeutral, StringComparison.OrdinalIgnoreCase) ? 1 : intensity;

            m_TargetTime = Time.time + kEmotionTransition;

            m_LipSync.ResetDisplayedEmotions();

            // Debug.Log($"Emotions: {m_LastEmotion} {m_LastEmotionStart*100:N0} -> {m_CurrentEmotion} {m_CurrentEmotionTarget*100:N0}");

            UpdateEmotions(0);
        }

        public void JumpToEmotion(string emotionName, float intensity) {
            m_ChangeEmotions = false;
            int emotionIndex = m_LipSync.emotions.IndexOf(e => Equals(e.emotion, emotionName));
            m_LipSync.ResetDisplayedEmotions();
            m_LipSync.DisplayEmotionPose(emotionIndex, intensity);
        }

        private void UpdateEmotions(float progress) {
            // Lower last emotion if there is one and it isn't the same as the current.
            if (!m_LastEmotion.IsNullOrEmpty() && !Equals(m_LastEmotion, m_CurrentEmotion)) {
                float lastProgress = Mathf.Lerp(m_LastEmotionStart, 0, progress);
                m_LipSync.DisplayEmotionPose(m_LastEmotionIndex, lastProgress);
                // Debug.Log($"Emotion {m_LastEmotion} -> {lastProgress:N2}", this);
            }

            // Increase current emotion
            m_CurrentEmotionProgress = Mathf.Lerp(m_CurrentEmotionStart, m_CurrentEmotionTarget, progress);
            m_LipSync.DisplayEmotionPose(m_CurrentEmotionIndex, m_CurrentEmotionProgress);
            // Debug.Log($"Emotion {m_CurrentEmotion} -> {m_CurrentEmotionProgress:N2}", this);
        }

        private static bool Equals(string a, string b) {
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        public override DelaySequence Perform(ScriptCommandInfo command) {
            // Find the performance name.
            string performanceName = command.Params.GetFirst();

            if (performanceName.IsNullOrEmpty()) {
                Debug.LogWarningFormat(this, "No performance name found.");
                return DelaySequence.Empty;
            }

            // Find the performance object.
            Object performance = Buckets.Performances.Items.FirstOrDefault(i => i && Equals(i.name, performanceName));

            if (performance == null) {
                Debug.LogWarningFormat(this, "Performance '{0}' not found.", performanceName);
                return DelaySequence.Empty;
            }

            return Play(performance);
        }

        public DelaySequence Play(Object performance) {
            if (!gameObject.activeInHierarchy) {
                Debug.LogWarning($"Character {name} disabled. Can't play {performance.name}", this);
                return DelaySequence.Empty;
            }

            CustomPerformance customPerformance = performance as CustomPerformance;

            if (customPerformance != null) {
                return customPerformance.Play(this);
            }

            // Handle individual animations.
            AnimationClip animClip = performance as AnimationClip;
            if (animClip != null) {
                return PlayAnim(animClip);
            }

            LipSyncData lipSyncData = performance as LipSyncData;
            if (lipSyncData != null) {
                return PlayLipSync(lipSyncData);
            }

            // Handle a timeline-based performance.
            TimelineAsset playableClip = performance as TimelineAsset;
            if (playableClip != null) {
                return PlayPlayable(playableClip);
            }

            Debug.LogErrorFormat(performance, "Performance type not recognized: " + performance.name);
            return DelaySequence.Empty;
        }

        public DelaySequence PlayAnim(AnimationClip clip, float delay = 0, float transition = -1, AnimationCurve lookWeight = null) {
            if (!m_Animator) {
                Debug.LogWarningFormat(this, "No animator found.");
                return DelaySequence.Empty;
            }

            // Creates AnimationClipPlayable and connects them to the mixer.
            if (!m_LastAnim.IsValid()) {
                m_LastAnim = GetClipPlayable(m_Graph, clip);
            }

            AnimationClipPlayable nextAnim = GetClipPlayable(m_Graph, clip);

            m_Graph.Disconnect(m_TransitionMixer, 0);
            m_Graph.Disconnect(m_TransitionMixer, 1);

            m_Graph.Connect(m_LastAnim, 0, m_TransitionMixer, 0);
            m_Graph.Connect(nextAnim, 0, m_TransitionMixer, 1);

            m_TransitionMixer.SetInputWeight(0, 1);
            m_TransitionMixer.SetInputWeight(1, 0);

            transition = transition >= 0 ? transition : 1;

            this.DOKill();
            m_Sequence = DOTween.Sequence();
            m_Sequence.SetTarget(this);
            m_Sequence.Append(DOTween.To(() => m_TransitionMixer.GetInputWeight(1), weight => {
                weight = Mathf.Clamp01(weight);
                m_TransitionMixer.SetInputWeight(0, 1.0f - weight);
                m_TransitionMixer.SetInputWeight(1, weight);
            }, 1, transition));

            if (m_LookAtIK) {
                IKSolverLookAt lookAt = m_LookAtIK.solver;

                if (lookWeight != null && clip != null) {
                    float progress = 0;
                    Sequence lookAtSequence = DOTween.Sequence();
                    lookAtSequence.SetTarget(this);
                    lookAtSequence.Append(DOTween.To(() => progress, weight => {
                        progress = weight;
                        lookAt.IKPositionWeight = lookWeight.Evaluate(weight);
                        // Debug.Log("Setting lookAt weight to " + lookAt.IKPositionWeight.ToString("N2"));
                    }, 1, clip.averageDuration)).SetLoops(clip.isLooping ? -1 : 0);
                } else {
                    m_Sequence.Join(DOTween.To(() => lookAt.IKPositionWeight, weight => { lookAt.IKPositionWeight = weight; }, 1, transition)).SetEase(Ease.InOutSine);
                }
            }

            m_LastAnim = nextAnim;
            return DelaySequence.Empty;
        }

        public DelaySequence PlayAudio(AudioClip clip, float delay = 0) {
            return Delay.For(delay, this).Then(() => m_VoiceSource.PlayOneShot(clip)).ThenWaitFor(clip.length);
        }

        public DelaySequence PlayLipSync(LipSyncData lipSyncData, float delay = 0) {
            return Delay.For(delay, this).Then(() => m_LipSync.Play(lipSyncData)).ThenWaitFor(lipSyncData.length);
        }

        public DelaySequence PlayPlayable(TimelineAsset playableAsset, float delay = 0) {
            Debug.LogWarning("PlayPlayable not implemented for Characters.");
            return DelaySequence.Empty;
        }
    }
}