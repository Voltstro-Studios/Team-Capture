// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Console;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;
using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Weapons
{
	internal class WeaponSway : MonoBehaviour
	{
		[ConVar("cl_sway_amount", "Sets how much a gun will sway", true)]
		public static float SwayAmount = 0.1f;

		[ConVar("cl_sway_enable", "Whether or not weapons will sway", true)]
		public static bool SwayEnabled = true;

		public float maxXAmount = 0.35f;
		public float maxYAmount = 0.2f;

		public float smooth = 3.0f;

		private float axisX;
		private float axisY;
		private Vector3 localPosition;

		private void Start()
		{
			localPosition = transform.localPosition;

			GameSettings.SettingsUpdated += ApplySettings;
			ApplySettings();
		}

		private void Update()
		{
			if (!SwayEnabled)
				return;

			if (ClientUI.IsPauseMenuOpen)
				return;

			float fx = -axisX * SwayAmount;
			float fy = -axisY * (SwayAmount - 0.05f);

			fx = Mathf.Clamp(fx, -maxXAmount, maxXAmount);
			fy = Mathf.Clamp(fy, -maxYAmount, maxYAmount);

			Vector3 detection = new Vector3(localPosition.x + fx, localPosition.y + fy, localPosition.z);
			transform.localPosition = Vector3.Lerp(transform.localPosition, detection, Time.deltaTime * smooth);
		}

		private void OnDestroy()
		{
			GameSettings.SettingsUpdated -= ApplySettings;
		}

		public void SetInput(float x, float y)
		{
			axisX = x;
			axisY = y;
		}

		private void ApplySettings()
		{
			MultiplayerSettingsClass multiplayerSettings = GameSettings.MultiplayerSettings;
			SwayEnabled = multiplayerSettings.WeaponSwayEnabled;
			SwayAmount = multiplayerSettings.WeaponSwayAmount;
		}
	}
}