using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scene", menuName = "Team Capture/TCScene")]
public class TCScene : ScriptableObject
{
	[Scene] public string sceneName;
	public string sceneNiceName;
}