using Team_Capture.Settings.Enums;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class VideoSettingsClass : Setting
	{
		[SettingsPropertyFormatName("Settings_VideoResolution")]
		public Resolution Resolution = Screen.currentResolution;

		[SettingsPropertyFormatName("Settings_VideoScreenMode")]
		public FullScreenMode ScreenMode = FullScreenMode.FullScreenWindow;

		[SettingsPropertyFormatName("Settings_VideoTextureQuality")]
		public TextureQuality TextureQuality = TextureQuality.FullRes;

		[SettingsPropertyFormatName("Settings_VideoVSync")]
		public VSync VSync = VSync.Disable;
	}
}