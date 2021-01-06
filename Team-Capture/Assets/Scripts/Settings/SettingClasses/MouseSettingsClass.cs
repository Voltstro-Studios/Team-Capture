using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal class MouseSettingsClass : Setting
	{
		[Range(50, 200)] [SettingsPropertyDisplayText("Settings_MouseSensitivity")]
		public int MouseSensitivity = 100;

		[SettingsPropertyDisplayText("Settings_MouseReverse")]
		public bool ReverseMouse;
	}
}