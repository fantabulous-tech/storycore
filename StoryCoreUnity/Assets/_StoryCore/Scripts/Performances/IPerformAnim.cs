using RogoDigital.Lipsync;
using StoryCore.Utils;
using UnityEngine;
using UnityEngine.Timeline;

namespace StoryCore {
    public interface IPerformAnim {
        DelaySequence PlayAnim(AnimationClip clip, float delay = 0, float transition = -1, AnimationCurve lookAtWeight = null);
    }

    public interface IPerformLipSync {
        DelaySequence PlayLipSync(LipSyncData lipSyncData, float delay = 0);
    }

    public interface IPerformPlayable {
        DelaySequence PlayPlayable(TimelineAsset playableAsset, float delay = 0);
    }

    public interface IPerformAudio {
        DelaySequence PlayAudio(AudioClip clip, float delay = 0);
    }
}