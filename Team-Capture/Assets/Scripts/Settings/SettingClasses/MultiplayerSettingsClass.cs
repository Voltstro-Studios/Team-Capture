using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal class MultiplayerSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Settings_MultiplayerMuzzleFlashLighting")]
		public bool WeaponMuzzleFlashLighting = true;

		[SettingsPropertyDisplayText("Settings_MultiplayerWeaponSway")]
		public bool WeaponSwayEnabled = true;

		[Range(0, 15)]
		[SettingsPropertyDisplayText("Settings_MultiplayerWeaponSwayAmount")]
		public float WeaponSwayAmount = 0.1f;
	}
}