// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.SceneManagement;
using UnityEngine;

namespace Team_Capture.BootManagement
{
	/// <summary>
	///		Loads a scene on boot, this is generally the last thing you want to do
	/// </summary>
	[CreateAssetMenu(fileName = "Scene Boot Item", menuName = "BootManager/Scene Boot Item")]
    internal sealed class LoadSceneBootItem : BootItem
    {
		/// <summary>
		///		The scene you want to load to
		/// </summary>
		[Tooltip("The scene you want to load to")]
	    [SerializeField] private TCScene sceneToLoadTo;

	    public override void OnBoot()
	    {
		    TCScenesManager.LoadScene(sceneToLoadTo);
	    }
    }
}