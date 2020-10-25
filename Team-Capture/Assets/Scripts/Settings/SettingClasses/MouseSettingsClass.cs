using UnityEngine;

namespace Settings.SettingClasses
{
	internal class MouseSettingsClass : Setting
	{
		[Range(50, 200)]
		[SettingsPropertyFormatName("Mouse Sensitivity")]
		public int MouseSensitivity = 100;

		[SettingsPropertyFormatName("Raw Axis")]
		public bool RawAxis = true;

		[SettingsPropertyFormatName("Reverse Mouse")]
		public bool ReverseMouse = false;
	}
}