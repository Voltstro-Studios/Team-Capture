using UnityEngine;

namespace Settings.SettingClasses
{
	internal class MultiplayerSettingsClass : Setting
	{
		[SettingsPropertyFormatName("Settings_MultiplayerMuzzleFlashLighting")]
		public bool WeaponMuzzleFlashLighting = true;

		[SettingsPropertyFormatName("Settings_MultiplayerWeaponSway")]
		public bool WeaponSwayEnabled = true;

		[Range(0, 15)]
		[SettingsPropertyFormatName("Settings_MultiplayerWeaponSwayAmount")]
		public float WeaponSwayAmount = 0.1f;
	}
}