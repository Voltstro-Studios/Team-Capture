using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
	public static class TCScenesManager
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitTcSceneManager()
		{
			SceneManager.sceneLoaded += UnitySceneManagerLoaded;
		}

		#region Loading Scenes

		public delegate void PreparingSceneLoad(TCScene scene);
		public delegate void StartSceneLoad(AsyncOperation sceneLoadOperation);
		public delegate void SceneLoaded(TCScene scene);

		/// <summary>
		/// An event that triggers when a new scene is requested to be loaded through <see cref="TCScenesManager"/>
		/// <para>This is so we get a bit of time to prepare other stuff, such as a loading screen.</para>
		/// </summary>
		public static event PreparingSceneLoad PreparingSceneLoadEvent;

		/// <summary>
		/// An event that triggers when a new scene is being loaded through <see cref="TCScenesManager"/>.
		/// </summary>
		public static event StartSceneLoad StartSceneLoadEvent;

		/// <summary>
		/// Called after a scene is loaded
		/// </summary>
		public static event SceneLoaded OnSceneLoadedEvent;

		/// <summary>
		/// Loads a scene
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
			if(tcScene == null) return;

			OnSceneLoadedEvent?.Invoke(tcScene);
		}

		#endregion

		#region Getting Scene

		/// <summary>
		/// Returns a list of all <see cref="TCScene"/> objects in the build (as long as they were in the resources folder).
		/// Includes disabled scenes
		/// </summary>
		/// <returns></returns>
		//FindObjectsOfType iss around 3x faster than LoadAll<T> (190ms vs 500ms), but might not work in the build
		public static IEnumerable<TCScene> GetAllTCScenesInfo()
		{
//			return Resources.LoadAll("").Where(r => r.GetType() == typeof(TCScene)).Select(t => (TCScene) t);
			return Resources.LoadAll<TCScene>("");
		}

		/// <summary>
		/// Gets all enabled scenes in the build
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TCScene> GetAllEnabledTCScenesInfo()
		{
			return GetAllTCScenesInfo().Where(s => s.enabled);
		}

		/// <summary>
		/// Finds a scene
		/// </summary>
		/// <param name="name">The name of the scene to find</param>
		/// <returns></returns>
		public static TCScene FindSceneInfo(string name)
		{
			return GetAllEnabledTCScenesInfo().FirstOrDefault(s => s.name == name);
		}

		public static TCScene GetActiveScene()
		{
			return FindSceneInfo(SceneManager.GetActiveScene().name);
		}

		#endregion
	}
}