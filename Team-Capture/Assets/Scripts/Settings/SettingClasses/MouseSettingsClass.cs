using Team_Capture.Settings;
using UnityEngine;

namespace Settings.SettingClasses
{
	internal class MouseSettingsClass : Setting
	{
		[Range(50, 200)] [SettingsPropertyFormatName("Settings_MouseSensitivity")]
		public int MouseSensitivity = 100;

		[SettingsPropertyFormatName("Settings_MouseReverse")]
		public bool ReverseMouse;
	}
}