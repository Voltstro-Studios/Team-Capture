using Attributes;
using Settings.Enums;
using UnityEngine;

namespace Settings.SettingClasses
{
	public sealed class VideoSettingsClass : Setting
	{
		public Resolution Resolution = Screen.currentResolution;
		
		[SettingsPropertyFormatName("Screen Mode")] public FullScreenMode ScreenMode = FullScreenMode.FullScreenWindow;

		[SettingsPropertyFormatName("Texture Quality")] public TextureQuality TextureQuality = TextureQuality.FullRes;

		public VSync VSync = VSync.Disable;
	}
}