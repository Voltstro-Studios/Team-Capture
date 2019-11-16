using Mirror;
using UnityEngine;

namespace SceneManagement
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