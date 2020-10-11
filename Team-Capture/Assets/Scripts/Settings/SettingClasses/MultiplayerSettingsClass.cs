using Attributes;
using UnityEngine;

namespace Settings.SettingClasses
{
	public class MultiplayerSettingsClass : Setting
	{
		[SettingsPropertyFormatName("Weapon Sway")]
		public bool WeaponSwayEnabled = true;

		[Range(0, 15)]
		[SettingsPropertyFormatName("Weapon Sway Amount")]
		public float WeaponSwayAmount = 0.1f;
	}
}