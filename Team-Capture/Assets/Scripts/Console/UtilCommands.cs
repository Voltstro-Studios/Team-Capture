using System;
using System.IO;
using Team_Capture.Core;
using Logger = Team_Capture.Core.Logging.Logger;
using Random = UnityEngine.Random;

namespace Team_Capture.Console
{
	/// <summary>
	///     A bunch of util commands
	/// </summary>
	internal static class UtilCommands
	{
		private const string SplashScreenResourceFile = "Resources/console-splashscreen.txt";

		[ConCommand("quit", "Quits the game")]
		public static void QuitGameCommand(string[] args)
		{
			Game.QuitGame();
		}

		[ConCommand("echo", "Echos back what you type in")]
		public static void EchoCommand(string[] args)
		{
			Logger.Info(string.Join(" ", args));
		}

		[ConCommand("asciiart", "Shows Team-Capture ascii art")]
		public static void AsciiArtCommand(string[] args)
		{
			//Ascii art, fuck you
			const string asciiArt = @"
___________                    
\__    ___/___ _____    _____  
  |    |_/ __ \\__  \  /     \ 
  |    |\  ___/ / __ \|  Y Y  \
  |____| \___  >____  /__|_|  /
             \/     \/      \/ 
	_________                __                        
	\_   ___ \_____  _______/  |_ __ _________   ____  
	/    \  \/\__  \ \____ \   __\  |  \_  __ \_/ __ \ 
	\     \____/ __ \|  |_> >  | |  |  /|  | \/\  ___/ 
	 \______  (____  /   __/|__| |____/ |__|    \___  >
	        \/     \/|__|                           \/ 
";
			Logger.Info(asciiArt);
		}

		[ConCommand("splashmessage", "Shows a random splash message")]
		public static void SplashMessageCommand(string[] args)
		{
			//Random splash message
			string splashMessagesPath = $"{Game.GetGameExecutePath()}/{SplashScreenResourceFile}";
			if (File.Exists(splashMessagesPath))
			{
				string[] lines = File.ReadAllLines(splashMessagesPath);

				//Select random number
				int index = Random.Range(0, lines.Length);
				Logger.Info($"	{lines[index]}");
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD

		[ConCommand("exception", "Manually causes an exception", CommandRunPermission.Both, 0, 1000)]
		public static void ManualExceptionCommand(string[] args)
		{
			try
			{
				string message = "Manual exception!";
				string argsJoined = string.Join(" ", args);
				if (!string.IsNullOrWhiteSpace(argsJoined))
					message = argsJoined;

				throw new Exception(message);
			}
			catch (Exception ex)
			{
				Logger.Error("{@Ex}", ex);
			}
		}

#endif
	}
}