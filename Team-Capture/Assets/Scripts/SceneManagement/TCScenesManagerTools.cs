using System;
using UnityEditor;
using UnityEngine;

namespace SceneManagement
{
	#if !UNITY_EDITOR
	public class MenuItemAttribute : System.Attribute
	{
		public MenuItemAttribute(string s)
		{
			
		}
	}
	#endif
	public static class TCScenesManagerTools
	{
		[MenuItem("Team Capture/List All Scenes")]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ListAllTCScenes()
		{
			Debug.Log($"{nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllTCScenes())
			{
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
		
		[MenuItem("Team Capture/List Enabled Scenes")]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ListAllEnabledScenes()
		{
			Debug.Log($"Enabled {nameof(TCScene)}s found:");
			foreach (TCScene tcScene in TCScenesManager.GetAllEnabledTCScenes())
			{	
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
		
		[MenuItem("Team Capture/Scenes test")]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void SearchForTest()
		{
			const string searchFor = "test_scene";
			TCScene scene = TCScenesManager.FindScene(searchFor);
			Debug.Log(
				scene == null ? $"\t\tCould not find test scene {searchFor}" : $"\t\tFound test scene {searchFor}");
		}
	}
}
