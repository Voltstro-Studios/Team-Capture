using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Scenes
{
	public static class TCScenesManager
	{
		private static readonly List<TCScene> Scenes;

		private const string ScenesFilePath = "/StreamingAssets/scenes.json";

		static TCScenesManager()
		{
			if (!File.Exists(Application.dataPath + ScenesFilePath))
			{
				Scenes = new List<TCScene>();
				SaveScenes();
			}
			else
			{
				Scenes = LoadScenes().ToList();


			}
		}

		/// <summary>
		/// Get a Team-Capture scene
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public static TCScene GetScene(string sceneName)
		{
			IEnumerable<TCScene> result = from a in Scenes
				where a.sceneName == sceneName
				select a;

			TCScene scene = result.FirstOrDefault();
			return scene;
		}

		/// <summary>
		/// Saves all scenes
		/// </summary>
		public static void SaveScenes()
		{
			Debug.Log(Scenes);

			string json = JsonConvert.SerializeObject(Scenes, Formatting.Indented);
			Debug.Log(json);

			File.WriteAllText(Application.dataPath + ScenesFilePath, json);

			Debug.Log("[Scenes] Saved scenes to file.");
		}

		/// <summary>
		/// Adds a scene to the list
		/// </summary>
		/// <param name="scene"></param>
		public static void AddScene(TCScene scene)
		{
			Scenes.Add(scene);
		}

		private static IEnumerable<TCScene> LoadScenes()
		{
			//TODO: Add MacOSX
			string json = File.ReadAllText(Application.dataPath + ScenesFilePath);

			//TODO: Figure out how to fix the 'TCScene must be instantiated using the ScriptableObject.CreateInstance method instead of new TCScene.' warning
		
			JObject tcSceneList = JObject.Parse(json);
			int length = tcSceneList.
			return JsonConvert.DeserializeObject<List<TCScene>>(json);
		}
	}
}
