// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.BootManagement
{
	internal enum RunOn : byte
	{
        GraphicsOnly,
        ServerOnly,
        Both
	}

	/// <summary>
	///		The base for a boot item
	/// </summary>
    internal class BootItem : ScriptableObject
    {
		/// <summary>
		///		What do you want this to run on?
		/// </summary>
		[Tooltip("What do you want this to run on?")]
	    [SerializeField] internal RunOn runOn = RunOn.Both;

		/// <summary>
		///		Called on boot when run conditions meet
		/// </summary>
	    public virtual void OnBoot()
	    {
	    }
    }
}