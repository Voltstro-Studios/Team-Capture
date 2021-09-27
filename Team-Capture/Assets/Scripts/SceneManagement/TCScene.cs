// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using Mirror;
using Team_Capture.Core;
using Team_Capture.Localization;
using Team_Capture.Weapons;
using UnityEngine;
using UnityEngine.Rendering;

namespace Team_Capture.SceneManagement
{
	/// <summary>
	///     Represents a Team-Capture scene data
	/// </summary>
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject
	{
		/// <summary>
		///     The actual Unity scene
		/// </summary>
		[Header("Basic Scene Settings")] [Scene]
		public string scene;

		/// <summary>
		///     The display name, the name that will be shown to the user
		/// </summary>
		[Tooltip("The display name, the name that will be shown to the user")] [SerializeField]
		private string displayName = "Map_DisplayName";

		/// <summary>
		///		Image that will be displayed while loading
		/// </summary>
		[Tooltip("Image that will be displayed while loading")]
		public Texture2D loadingScreenBackgroundImage;

		/// <summary>
		///     Will this scene be included in the build?
		/// </summary>
		[Tooltip("Will this scene be included in the build?")]
		public bool enabled = true;

		/// <summary>
		///     Is this scene an online scene, the only offline one should be the Main Menu or Bootloader and such
		/// </summary>
		[Tooltip("Is this scene an online scene, the only offline one should be the Main Menu or Bootloader and such")]
		public bool isOnlineScene = true;

		/// <summary>
		///     Can this scene be loaded to directly (Using the scene command)
		/// </summary>
		[Tooltip("Can this scene be loaded to directly (Using the scene command)")]
		public bool canLoadTo = true;

		/// <summary>
		///		<see cref="VolumeProfile"/> containing overriding settings
		///		<para>The settings are applied on top of the existing settings.</para>
		/// </summary>
		public VolumeProfile overrideProfile;

		/// <summary>
		///     The time until the players respawn
		/// </summary>
		[Header("Game Settings")] 
		[Tooltip("The time until the players respawn")]
		public float respawnTime = 4.0f;

		/// <summary>
		///     The stock weapons to add to player
		/// </summary>
		[Header("Weapon Settings")] 
		public TCWeapon[] stockWeapons;

		public GameObject traceEffectPrefab;

		public GameObject bulletHoleEffectPrefab;

		/// <summary>
		///     The scene, but only its name (So no 'Assets/Scenes/*.unity' stuff)
		/// </summary>
		public string SceneFileName => Path.GetFileNameWithoutExtension(scene);

		/// <summary>
		///     The display name, the name that will be shown to the user (Localized)
		/// </summary>
		public string DisplayNameLocalized =>
			displayNameLocalized ?? (displayNameLocalized = ResolveMapString(displayName));

		#region Discord RPC

		/// <summary>
		///     Do you want to show the start time on Discord RPC
		/// </summary>
		[Header("Discord RPC")] public bool showStartTime = true;

		/// <summary>
		///     Is this the main menu?
		/// </summary>
		[Tooltip("Is this the main menu?")] public bool isMainMenu;

		/// <summary>
		///     What large image to use
		/// </summary>
		[Tooltip("What large image to use")] public string largeImageKey = "tc_icon";

		/// <summary>
		///     What text will the large image have
		/// </summary>
		[Tooltip("What text will the large image have")] [SerializeField]
		private string largeImageKeyText = "Discord_LargeImageKeyText";

		/// <summary>
		///     Gets the localized version of <see cref="largeImageKeyText" />
		/// </summary>
		public string LargeImageKeyTextLocalized => largeImageKeyTextLocalized ??
		                                            (largeImageKeyTextLocalized = ResolveMapString(largeImageKeyText));

		#endregion

		#region Locales

		[NonSerialized] private string largeImageKeyTextLocalized;
		[NonSerialized] private string displayNameLocalized;

		private Locale sceneLocale;

		public string ResolveMapString(string id)
		{
			if (sceneLocale == null)
				sceneLocale = new Locale($"{Game.GetGameExecutePath()}/Resources/Maps/{SceneFileName}-%LANG%.json");

			return sceneLocale.ResolveString(id);
		}

		#endregion
	}
}