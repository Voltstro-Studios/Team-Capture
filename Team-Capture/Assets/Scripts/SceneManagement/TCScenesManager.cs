using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Scenes;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SceneManagement
{
	public static class TCScenesManager
	{

		[MenuItem("Team Capture/List all TC scenes")]
		private static void ListAllTCScenes()
		{
			Debug.Log($"{nameof(TCScene)}s found:");
			foreach (TCScene tcScene in GetAllTCScenes())
			{
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}
		
		[MenuItem("Team Capture/List all enabled TC scenes")]
		private static void ListAllEnabledScenes()
		{
			Debug.Log($"Enabled {nameof(TCScene)}s found:");
			foreach (TCScene tcScene in GetAllTCScenes())
			{
				Debug.Log($"\t{tcScene.sceneName} ({tcScene.displayName})");
			}
		}

		/// <summary>
		/// Returns a list of all <see cref="TCScene"/> objects in the build (as long as they were in the resources folder). Includes disabled scenes
		/// </summary>
		/// <returns></returns>
		//Use FindObjectsOfType over LoadAll<T> because it's around 3x faster (190ms vs 500ms)
		private static IEnumerable<TCScene> GetAllTCScenes() => Resources.FindObjectsOfTypeAll<TCScene>();

		/// <summary>
		/// Gets all enabled scenes in the build
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<TCScene> GetAllEnabledTCScenes() => GetAllTCScenes().Where(s => s.enabled);
	}
}