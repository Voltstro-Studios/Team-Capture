using Mirror;
using UnityEngine;
using Weapons;

namespace SceneManagement
{
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject
	{
		[Header("Basic Scene Settings")]
		[Scene] public string sceneName;

		[Tooltip("Its 'nice' name")]
		public string displayName;

		[Tooltip("Is this scene playable?")] 
		public bool enabled = true;

		[Header("Game Settings")]
		public GamemodeSettings gamemodeSettings;
		public float respawnTime = 4.0f;

		[Tooltip("What weapons do you want the player to start with?")]
		public TCWeapon[] stockWeapons;

		[Header("Temp Stuff")]
		public float hitObjectLastTime = 2.0f;
		public GameObject weaponHit;
	}
}