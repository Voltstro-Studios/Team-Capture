using UnityEditor;
using UnityEngine;

namespace SceneManagement.Editor
{
	public class TCScenesManagerTools
	{
		[MenuItem("Team Capture/List All Scenes")]
		private static void ListAllTCScenes()
		{
			Debug.Log($"{nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllTCScenes())
			{
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
		
		[MenuItem("Team Capture/List Enabled Scenes")]
		private static void ListAllEnabledScenes()
		{
			Debug.Log($"Enabled {nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllEnabledTCScenes())
			{	
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}

		[MenuItem("Team Capture/Scenes test")]
		private static void SearchForTest()
		{
			const string searchFor = "search_for";
			Debug.Log(TCScenesManager.FindScene(searchFor));
		}
	}
}
