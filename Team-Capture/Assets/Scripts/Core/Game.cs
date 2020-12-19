using System;
using System.IO;
using Console;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = Core.Logging.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public static class Game
	{
		/// <summary>
		///     Whether or not we are running in headless mode
		/// </summary>
		public static bool IsHeadless => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

		public static bool IsGameQuitting { get; private set; }

		/// <summary>
		///     Quits the game
		/// </summary>
		public static void QuitGame()
		{
			IsGameQuitting = true;

			if (NetworkManager.singleton != null)
				NetworkManager.singleton.StopHost();

			Logger.Info("Goodbye!");

			ConsoleSetup.ConsoleUI?.Shutdown();

#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		/// <summary>
		///     Gets the path where the game's exe is
		///     <para>(Or with the editor is is under the `/Game` folder)</para>
		/// </summary>
		/// <returns></returns>
		public static string GetGameExecutePath()
		{
#if UNITY_EDITOR
			return Directory.GetParent(Application.dataPath).FullName + "/Game";
#else
			return Directory.GetParent(Application.dataPath).FullName;
#endif
		}

		/// <summary>
		///     Get the path used for settings
		/// </summary>
		/// <returns></returns>
		public static string GetGameConfigPath()
		{
			//For Windows we store our settings into the documents
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\";

			//And make sure the 'My Games' folder exists
			if (!Directory.Exists(documentsFolder))
				Directory.CreateDirectory(documentsFolder);

			//Add on our game name
			documentsFolder += Application.productName + @"\";
#else
			//But on UNIX we don't
			string documentsFolder = $"{GetGameExecutePath()}/Settings/";
#endif

			//And make sure our game name folder exists as well
			if (!Directory.Exists(documentsFolder))
				Directory.CreateDirectory(documentsFolder);

			return documentsFolder;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			Application.quitting += () => IsGameQuitting = true;
		}
	}
}