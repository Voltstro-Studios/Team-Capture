using Mirror;
using UnityEngine;
using Weapons;

namespace SceneManagement
{
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject	
	{
		[Scene] public string sceneName;
		public string displayName;
		[Tooltip("Is this scene playable?")]
		public bool enabled = true;

		public GamemodeSettings gamemodeSettings;

		public TCWeapon[] stockWeapons;
	}
}
