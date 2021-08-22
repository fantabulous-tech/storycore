using CoreUtils;
using RogoDigital.Lipsync;
using UnityEngine;
using UnityEngine.Timeline;

namespace StoryCore {
    public delegate void PerformProgressDelegate(object sender, AnimationClip clip, float progress, float weight);

    public interface IPerformAnim {
        DelaySequence PlayAnim(AnimationClip clip, float delay = 0, float transition = -1, PerformProgressDelegate progressCallback = null);
    }

    public interface IPerformLipSync {
        DelaySequence PlayLipSync(LipSyncData lipSyncData, float delay = 0);
        void StopLipSync();
    }

    public interface IPerformPlayable {
        DelaySequence PlayPlayable(TimelineAsset playableAsset, float delay = 0);
    }

    public interface IPerformAudio {
        DelaySequence PlayAudio(AudioClip clip, float delay = 0);
    }
}