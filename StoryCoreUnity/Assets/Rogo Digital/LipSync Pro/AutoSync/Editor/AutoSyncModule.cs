using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogoDigital.Lipsync.AutoSync
{
	[AutoSyncModuleInfo("An Unknown AutoSync Module.")]
	public abstract class AutoSyncModule : ScriptableObject
	{
		/// <summary>
		/// Returns a bitmask of compatibility criteria that must be met in order for this module to run.
		/// </summary>
		/// <returns></returns>
		public abstract ClipFeatures GetCompatibilityRequirements ();

		/// <summary>
		/// Returns a bitmask of compatibility criteria that will be fulfilled by this module.
		/// </summary>
		/// <returns></returns>
		public abstract ClipFeatures GetOutputCompatibility ();

		/// <summary>
		/// Begins processing the supplied inputClip asynchronously using this module's settings, and will call the supplied callback when finished.
		/// </summary>
		/// <param name="inputClip"></param>
		/// <param name="callback"></param>
		/// <param name="customData"></param>
		public abstract void Process (LipSyncData inputClip, AutoSync.ASProcessDelegate callback);
	}
}