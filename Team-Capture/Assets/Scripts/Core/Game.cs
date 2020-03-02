using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public static class Game
	{
		/// <summary>
		/// Quits the game
		/// </summary>
		public static void QuitGame()
		{
			Logger.Logger.Log("Goodbye!");

#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		public static string GetGameExecutePath()
		{
#if UNITY_EDITOR
			return Directory.GetParent(Application.dataPath).FullName + "/Game";
#else
			return Directory.GetParent(Application.dataPath).FullName;
#endif
		}
	}
}
