using UnityEditor;
using UnityEngine;

namespace SceneManagement
{
	public static class TCScenesManagerTools
	{
		[MenuItem("Team Capture/List All Scenes")]
		private static void ListAllTCScenes()
		{
			Debug.Log($"{nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllTCScenes())
			{
				Debug.Log($"{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
		
		[MenuItem("Team Capture/List Enabled Scenes")]
		private static void ListAllEnabledScenes()
		{
			Debug.Log($"Enabled {nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllEnabledTCScenes())
			{	
				Debug.Log($"{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
	}
}
