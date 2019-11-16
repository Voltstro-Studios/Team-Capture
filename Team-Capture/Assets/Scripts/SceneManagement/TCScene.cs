using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
	[CreateAssetMenu(fileName = "New TC Scene", menuName = "Team Capture/TCScene")]
	public class TCScene : ScriptableObject	
	{
		[Scene] public string sceneName;
		public string displayName;
		[Tooltip("Is this scene playable?")]
		public bool enabled = true;
	}
}