// ReSharper disable once RedundantUsingDirective
using System;
using System.IO;
using Mirror;
using Team_Capture.Console;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = Team_Capture.Logging.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team_Capture.Core
{
	/// <summary>
	///		Some core functions relating to the game
	/// </summary>
	public static class Game
	{
		/// <summary>
		///     Whether or not we are running in headless mode
		/// </summary>
		public static bool IsHeadless => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

		/// <summary>
		///		Is the game in a quitting state
		/// </summary>
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
		///     <para>(Or with the editor it is under the `/Game` folder)</para>
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
#if UNITY_EDITOR_WINDOWS || UNITY_STANDALONE_WIN //For Windows we store our config in their documents under 'My Games'
			string configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/My Games/";
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX //For Linux we store our config in their home directory's .config folder
			string configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.config/";
#endif		//TODO: Figure out where to store config files on MacOS, I would assume it be the same as Linux but need to double check
			
			//And make sure the config path exists
			if (!Directory.Exists(configPath))
				Directory.CreateDirectory(configPath);

			//Add on our game name
			configPath += Application.productName + @"/";

			//And make sure our game name folder exists as well
			if (!Directory.Exists(configPath))
				Directory.CreateDirectory(configPath);

			return configPath;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			Application.quitting += () => IsGameQuitting = true;
		}
	}
}