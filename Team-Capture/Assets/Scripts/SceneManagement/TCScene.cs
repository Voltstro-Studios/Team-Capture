using Mirror.Attributes;
using UnityEngine;

namespace SceneManagement
{
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject
	{
		[Header("Basic Scene Settings")] 
		[Scene] public string scene;

		[Tooltip("Its 'nice' name")] 
		public string displayName;

		[Tooltip("Will this scene be included in the build?")] 
		public bool enabled = true;

		[Header("Game Settings")] 
		public GamemodeSettings gamemodeSettings;
		
		public float respawnTime = 4.0f;

		[Header("Temp Stuff")] 
		public float hitObjectLastTime = 2.0f;
		public GameObject weaponHit;
	}
}