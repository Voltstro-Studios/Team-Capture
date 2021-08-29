// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using Newtonsoft.Json;
using Team_Capture.Core;
using Team_Capture.Helper;

namespace Team_Capture.Integrations.Steamworks
{
	[Serializable]
	internal class SteamSettings
	{
		/// <summary>
		///     Where to load the settings from
		/// </summary>
		private const string SettingsLocation = "/Resources/Integrations/Steam.json";
		
		private static SteamSettings steamSettings;
		
		[JsonIgnore]
		public static SteamSettings SteamSettingsInstance
		{
			get
			{
				return steamSettings ??= ObjectSerializer.LoadJson<SteamSettings>(
					Path.GetDirectoryName($"{Game.GetGameExecutePath()}{SettingsLocation}"),
					$"/{Path.GetFileNameWithoutExtension(SettingsLocation)}");
			}
		}
		
		/// <summary>
		///		AppID for Steam to connect to
		/// </summary>
		public uint appId;

		/// <summary>
		///		Dedicated server app ID
		/// </summary>
		public uint appDedicatedServerId;
	}
}