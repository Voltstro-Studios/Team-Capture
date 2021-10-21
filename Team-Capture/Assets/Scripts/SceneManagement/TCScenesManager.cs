// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Mirror;
using Team_Capture.Console;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Logger = Team_Capture.Logging.Logger;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Team_Capture.SceneManagement
{
	/// <summary>
	///     Scene Manager for Team-Capture
	/// </summary>
	public static class TCScenesManager
	{
		private const string SceneLabel = "Scene";
		
		private static IList<TCScene> scenes;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitTcSceneManager()
		{
			SceneManager.sceneLoaded += UnitySceneManagerLoaded;

			LoadAllScenes();
		}

		[ConCommand("scene", "Loads a scene", CommandRunPermission.Both, 1, 1)]
		public static void LoadSceneCommand(string[] args)
		{
			NetworkManagerMode mode = NetworkManager.singleton.mode;

			TCScene scene = FindSceneInfo(args[0]);

			//Scene doesn't exist
			if (scene == null)
			{
				Logger.Error("The scene '{@Scene}' doesn't exist!", args[0]);
				return;
			}

			//This scene cannot be loaded to
			if (!scene.canLoadTo)
			{
				Logger.Error("You cannot load to this scene!");
				return;
			}

			switch (mode)
			{
				//We are in client mode
				case NetworkManagerMode.ClientOnly:
					//Disconnect from current server
					NetworkManager.singleton.StopHost();

					//Load the scene
					Logger.Info("Changing scene to {@Scene}...", scene.scene);
					LoadScene(scene);
					break;

				//We are server, so we will tell the server and clients to change scene
				case NetworkManagerMode.ServerOnly:
					Logger.Info("Changing scene to {@Scene}...", scene.scene);
					NetworkManager.singleton.ServerChangeScene(scene.scene);
					break;

				case NetworkManagerMode.Offline:
					Logger.Info("Changing scene to {@Scene}...", scene.scene);
					LoadScene(scene);
					break;
			}
		}

		#region Loading Scenes

		public delegate void PreparingSceneLoad(TCScene scene);

		public delegate void StartSceneLoad(AsyncOperation sceneLoadOperation);

		public delegate void SceneLoaded(TCScene scene);

		/// <summary>
		///     An event that triggers when a new scene is requested to be loaded through <see cref="TCScenesManager" />
		///     <para>This is so we get a bit of time to prepare other stuff, such as a loading screen.</para>
		/// </summary>
		public static event PreparingSceneLoad PreparingSceneLoadEvent;

		/// <summary>
		///     An event that triggers when a new scene is being loaded through <see cref="TCScenesManager" />.
		/// </summary>
		public static event StartSceneLoad StartSceneLoadEvent;

		/// <summary>
		///     Called after a scene is loaded
		/// </summary>
		public static event SceneLoaded OnSceneLoadedEvent;

		/// <summary>
		///     Loads a scene
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="loadMode"></param>
		/// <returns></returns>
		public static AsyncOperation LoadScene(TCScene scene, LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			PreparingSceneLoadEvent?.Invoke(scene);
			Debug.Log($"The scene `{scene.scene}` was requested to be loaded.");

			AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(scene.scene, loadMode);

			StartSceneLoadEvent?.Invoke(sceneLoad);

			return sceneLoad;
		}

		private static void UnitySceneManagerLoaded(Scene scene, LoadSceneMode mode)
		{
			TCScene tcScene = FindSceneInfo(scene.name);
			if (tcScene == null) return;

			OnSceneLoadedEvent?.Invoke(tcScene);
		}

		#endregion

		#region Getting Scene

		private static void LoadAllScenes()
		{
			scenes = Addressables.LoadAssetsAsync<TCScene>(SceneLabel, null).WaitForCompletion();
		}

		public static IList<TCScene> GetAllScenes()
		{
			if(scenes == null)
				LoadAllScenes();
			
			return scenes;
		}

		public static List<TCScene> GetAllOnlineScenes()
		{
			if(scenes == null)
				LoadAllScenes();
			
			return GetAllScenes().Where(x => x.isOnlineScene).ToList();
		}

		/// <summary>
		///     Finds a scene
		/// </summary>
		/// <param name="name">The name of the scene to find</param>
		/// <returns></returns>
		public static TCScene FindSceneInfo(string name)
		{
			return scenes.FirstOrDefault(s => s.name == name);
		}

		/// <summary>
		///     Gets the current active scene
		/// </summary>
		/// <returns></returns>
		public static TCScene GetActiveScene()
		{
			return FindSceneInfo(SceneManager.GetActiveScene().name);
		}

		#endregion
	}
}