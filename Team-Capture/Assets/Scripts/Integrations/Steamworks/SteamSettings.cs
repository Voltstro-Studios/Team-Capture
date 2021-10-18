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
using UnityEngine;

namespace Team_Capture.Integrations.Steamworks
{
	[CreateAssetMenu(fileName = "Steam Settings", menuName = "Team Capture/Steam Settings")]
	internal class SteamSettings : ScriptableObject
	{
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