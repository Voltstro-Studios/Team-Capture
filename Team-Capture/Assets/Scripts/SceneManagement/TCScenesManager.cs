using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SceneManagement
{
	public static class TCScenesManager
	{
		/// <summary>
		/// Returns a list of all <see cref="TCScene"/> objects in the build (as long as they were in the resources folder). Includes disabled scenes
		/// </summary>
		/// <returns></returns>
		//FindObjectsOfType iss around 3x faster than LoadAll<T> (190ms vs 500ms), but might not work in the build
		public static IEnumerable<TCScene> GetAllTCScenes()
		{
//			return Resources.LoadAll("").Where(r => r.GetType() == typeof(TCScene)).Select(t => (TCScene) t);
			return Resources.LoadAll<TCScene>("");
		}

		/// <summary>
		/// Gets all enabled scenes in the build
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TCScene> GetAllEnabledTCScenes() => GetAllTCScenes().Where(s => s.enabled); 

		/// <summary>
		/// Finds a scene
		/// </summary>
		/// <param name="name">The name of the scene to find</param>
		/// <returns></returns>
		public static TCScene FindScene(string name) => GetAllTCScenes().FirstOrDefault(s => s.name == name);
	}
}
