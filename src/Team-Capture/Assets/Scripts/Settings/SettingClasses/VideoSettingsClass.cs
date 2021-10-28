// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Settings.Enums;
using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal sealed class VideoSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Text", "Settings_Video_Resolution")]
		public Resolution Resolution = Screen.currentResolution;

		[SettingsPropertyDisplayText("Text", "Settings_Video_ScreenMode")]
		public ScreenMode ScreenMode = ScreenMode.Fullscreen;

		[SettingsPropertyDisplayText("Text", "Settings_Video_TextureQuality")]
		public TextureQuality TextureQuality = TextureQuality.FullRes;

		[SettingsPropertyDisplayText("Text", "Settings_Video_VSync")]
		public VSync VSync = VSync.Disable;
	}
}