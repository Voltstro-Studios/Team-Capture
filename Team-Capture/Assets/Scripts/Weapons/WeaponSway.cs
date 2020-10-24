using Console;
using Settings;
using Settings.SettingClasses;
using UI;
using UnityEngine;

namespace Weapons
{
	internal class WeaponSway : MonoBehaviour
	{
		private Vector3 localPosition;

		[ConVar("cl_sway_amount", "Sets how much a gun will sway", true)]
		public static float SwayAmount = 0.1f;

		[ConVar("cl_sway_enable", "Whether or not weapons will sway", true)]
		public static bool SwayEnabled = true;

		public float maxXAmount = 0.35f;
		public float maxYAmount = 0.2f;

		public float smooth = 3.0f;

		private float axisX;
		private float axisY;

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

		private void Start()
		{
			localPosition = transform.localPosition;

			GameSettings.SettingsUpdated += ApplySettings;
			ApplySettings();
		}

		private void OnDestroy()
		{
			GameSettings.SettingsUpdated -= ApplySettings;
		}

		private void Update()
		{
			if(!SwayEnabled)
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
	}
}