using Mirror.Attributes;
using UnityEngine;

namespace SceneManagement
{
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject
	{
		[Header("Basic Scene Settings")] 
		[Scene] public string scene;

		//TODO: This shouldn't really be hard coded, as it forces all scenes into the scenes folder (not that its bad for all scenes to be in that folder)
		public string ScenePath => $"Assets/Scenes/{scene}.unity";

		[Tooltip("Its 'nice' name")] 
		public string displayName;

		[Tooltip("Will this scene be included in the build?")] 
		public bool enabled = true;

		[Tooltip("Is this scene an online scene, the only offline one should be Main Menus and such")]
		public bool isOnlineScene = true;

		public bool canLoadTo = true;

		public string pickupsParent = "/Pickups/";

		[Header("Game Settings")] 
		public GamemodeSettings gamemodeSettings;
		
		public float respawnTime = 4.0f;

		[Header("Discord RPC")] 
		public bool showStartTime = true;

		public bool isMainMenu;

		public string largeImageKey = "tc_icon";
		public string largeImageKeyText = "Team Capture";
	}
}