using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogoDigital.Lipsync.AutoSync
{
	[AutoSyncModuleInfo("Phoneme Modification/Intensity From Volume Module", "Sets the intensity of phoneme markers according to the volume of the audio.", "Rogo Digital")]
	public class ASPhonemeIntensityFromVolumeModule : AutoSyncModule
	{
		public AnimationCurve remapCurve = new AnimationCurve();

		public override ClipFeatures GetCompatibilityRequirements ()
		{
			return ClipFeatures.Phonemes | ClipFeatures.AudioClip;
		}

		public override ClipFeatures GetOutputCompatibility ()
		{
			return ClipFeatures.None;
		}

		public override void Process (LipSyncData inputClip, AutoSync.ASProcessDelegate callback)
		{
			for (int m = 0; m < inputClip.phonemeData.Length; m++)
			{
				inputClip.phonemeData[m].intensity = remapCurve.Evaluate(GetRMS(4096, Mathf.RoundToInt(inputClip.phonemeData[m].time * inputClip.clip.samples), inputClip.clip));
			}

			callback.Invoke(inputClip, new AutoSync.ASProcessDelegateData(true, "", ClipFeatures.None));
		}

		float GetRMS (int samples, int offset, AudioClip clip)
		{
			float[] sampleData = new float[samples];

			clip.GetData(sampleData, offset); // fill array with samples

			float sum = 0;
			for (int i = 0; i < samples; i++)
			{
				sum += sampleData[i] * sampleData[i]; // sum squared samples
			}

			return Mathf.Sqrt(sum / samples); // rms = square root of average
		}
	}
}