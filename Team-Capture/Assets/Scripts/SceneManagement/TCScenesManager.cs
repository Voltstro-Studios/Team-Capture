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
		private static List<TCScene> scenes = new List<TCScene>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitTcSceneManager()
		{
			SceneManager.sceneLoaded += UnitySceneManagerLoaded;

			scenes = GetAllEnabledTCScenesInfo().ToList();
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

		/// <summary>
		///     Returns a list of all <see cref="TCScene" /> objects in the build (as long as they were in the resources folder).
		///     Includes disabled scenes
		/// </summary>
		/// <returns></returns>
		//FindObjectsOfType iss around 3x faster than LoadAll<T> (190ms vs 500ms), but might not work in the build
		public static IEnumerable<TCScene> GetAllTCScenesInfo()
		{
			return Resources.LoadAll<TCScene>("");
		}

		/// <summary>
		///     Gets all enabled scenes in the build
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TCScene> GetAllEnabledTCScenesInfo()
		{
			return GetAllTCScenesInfo().Where(s => s.enabled);
		}

		/// <summary>
		///     Gets all enabled, online scenes
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TCScene> GetAllEnabledOnlineScenesInfo()
		{
			return GetAllEnabledTCScenesInfo().Where(s => s.isOnlineScene);
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