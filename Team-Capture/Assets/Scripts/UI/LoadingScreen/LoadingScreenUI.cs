// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.LoadingScreen
{
	/// <summary>
	///     A panel for a loading screen
	/// </summary>
	internal class LoadingScreenUI : MonoBehaviour
	{
		[SerializeField] private Slider slider;
		[SerializeField] private RawImage backgroundImage;
		[SerializeField] private TextMeshProUGUI mapTex;

		/// <summary>
		///		Sets up the loading screen
		/// </summary>
		/// <param name="scene"></param>
		public void Setup(TCScene scene)
		{
			if (scene.loadingScreenBackgroundImage != null)
				backgroundImage.texture = scene.loadingScreenBackgroundImage;

			mapTex.text = scene.DisplayNameLocalized;
		}

		/// <summary>
		///     Sets the loading bar amount
		/// </summary>
		/// <param name="amount"></param>
		public void SetLoadingBarAmount(float amount)
		{
			slider.value = amount;
		}
	}
}