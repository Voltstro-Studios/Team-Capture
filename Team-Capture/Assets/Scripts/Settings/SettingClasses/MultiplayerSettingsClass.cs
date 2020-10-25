using UnityEngine;

namespace Settings.SettingClasses
{
	internal class MultiplayerSettingsClass : Setting
	{
		[SettingsPropertyFormatName("Muzzle Flash Lighting")]
		public bool WeaponMuzzleFlashLighting = true;

		[SettingsPropertyFormatName("Weapon Sway")]
		public bool WeaponSwayEnabled = true;

		[Range(0, 15)]
		[SettingsPropertyFormatName("Weapon Sway Amount")]
		public float WeaponSwayAmount = 0.1f;
	}
}