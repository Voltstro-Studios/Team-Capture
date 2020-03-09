using UnityEngine;

namespace Settings.SettingClasses
{
	public sealed class VideoSettingsClass : Setting
	{
		public Resolution Resolution = Screen.currentResolution;

		public FullScreenMode ScreenMode = FullScreenMode.FullScreenWindow;
	}
}