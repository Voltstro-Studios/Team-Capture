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
		//Use FindObjectsOfType over LoadAll<T> because it's around 3x faster (190ms vs 500ms)
		public static IEnumerable<TCScene> GetAllTCScenes() => Resources.FindObjectsOfTypeAll<TCScene>();

		/// <summary>
		/// Gets all enabled scenes in the build
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TCScene> GetAllEnabledTCScenes() => GetAllTCScenes().Where(s => s.enabled); 

		/// <summary>
		/// Finds a scene
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static TCScene FindScene(string name)
		{
			return GetAllTCScenes().FirstOrDefault(s => s.name == name);
		}
	}
}
