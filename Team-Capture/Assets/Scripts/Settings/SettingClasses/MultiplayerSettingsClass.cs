// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Core.Networking;
using Team_Capture.UI;
using UnityEngine;

namespace Team_Capture.Settings.SettingClasses
{
	internal class MultiplayerSettingsClass : Setting
	{
		[SettingsPropertyDisplayText("Text", "Settings_Multiplayer_MuzzleFlashLighting")]
		public bool WeaponMuzzleFlashLighting = true;

		[SettingsPropertyDisplayText("Text", "Settings_Multiplayer_WeaponSway")]
		public bool WeaponSwayEnabled = true;

		[Range(0, 15)]
		[SettingsPropertyDisplayText("Text", "Settings_Multiplayer_WeaponSwayAmount")]
		public float WeaponSwayAmount = 0.1f;

		[SettingsPropertyDisplayText("Text", "Settings_Multiplayer_MOTDMode")]
		public Client.ClientMOTDMode MOTDMode = Client.ClientMOTDMode.WebSupport;
	}
}