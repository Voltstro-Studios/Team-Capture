using Team_Capture.Settings.Enums;
using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class VideoSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Settings_VideoResolution")]
		public Resolution Resolution = Screen.currentResolution;

		[SettingsPropertyDisplayText("Settings_VideoScreenMode")]
		public FullScreenMode ScreenMode = FullScreenMode.FullScreenWindow;

		[SettingsPropertyDisplayText("Settings_VideoTextureQuality")]
		public TextureQuality TextureQuality = TextureQuality.FullRes;

		[SettingsPropertyDisplayText("Settings_VideoVSync")]
		public VSync VSync = VSync.Disable;
	}
}