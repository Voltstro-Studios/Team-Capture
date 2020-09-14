using System.IO;
using Mirror;
using UnityEngine;
using Weapons;

namespace SceneManagement
{
	/// <summary>
	/// Represents a Team-Capture scene data
	/// </summary>
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject
	{
		/// <summary>
		/// The actual Unity scene
		/// </summary>
		[Header("Basic Scene Settings")] 
		[Scene] public string scene;

		/// <summary>
		/// The scene, but only its name (So no 'Assets/Scenes/*.unity' stuff)
		/// </summary>
		public string SceneFileName => Path.GetFileNameWithoutExtension(scene);

		/// <summary>
		/// The display name, the name that will be shown to the user
		/// </summary>
		[Tooltip("The display name, the name that will be shown to the user")] 
		public string displayName;

		/// <summary>
		/// Will this scene be included in the build?
		/// </summary>
		[Tooltip("Will this scene be included in the build?")] 
		public bool enabled = true;

		/// <summary>
		/// Is this scene an online scene, the only offline one should be the Main Menu or Bootloader and such
		/// </summary>
		[Tooltip("Is this scene an online scene, the only offline one should be the Main Menu or Bootloader and such")]
		public bool isOnlineScene = true;

		/// <summary>
		/// Can this scene be loaded to directly (Using the scene command)
		/// </summary>
		[Tooltip("Can this scene be loaded to directly (Using the scene command)")]
		public bool canLoadTo = true;

		/// <summary>
		/// The parent object that will hold all the pickups
		/// </summary>
		[Tooltip("The parent object that will hold all the pickups")]
		public string pickupsParent = "/Pickups/";

		/// <summary>
		/// The time until the players respawn
		/// </summary>
		[Header("Game Settings")]
		[Tooltip("The time until the players respawn")]
		public float respawnTime = 4.0f;

		/// <summary>
		/// The stock weapons to add to player
		/// </summary>
		public TCWeapon[] stockWeapons;

		/// <summary>
		/// Do you want to show the start time on Discord RPC
		/// </summary>
		[Header("Discord RPC")] 
		public bool showStartTime = true;

		/// <summary>
		/// Is this the main menu?
		/// </summary>
		[Tooltip("Is this the main menu?")]
		public bool isMainMenu;

		/// <summary>
		/// What large image to use
		/// </summary>
		[Tooltip("What large image to use")]
		public string largeImageKey = "tc_icon";

		/// <summary>
		/// What text will the large image have
		/// </summary>
		[Tooltip("What text will the large image have")]
		public string largeImageKeyText = "Team Capture";
	}
}