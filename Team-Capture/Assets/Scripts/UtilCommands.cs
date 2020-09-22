using System.IO;
using Attributes;
using Core;
using Core.Logging;
using Mirror;
using SceneManagement;

/// <summary>
/// A bunch of util commands
/// </summary>
public static class UtilCommands
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

	[ConCommand("scene", "Loads a scene", 1, 1)]
	public static void LoadScene(string[] args)
	{
		NetworkManagerMode mode = NetworkManager.singleton.mode;

		TCScene scene = TCScenesManager.FindSceneInfo(args[0]);

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
				TCScenesManager.LoadScene(scene);
				break;

			//We are server, so we will tell the server and clients to change scene
			case NetworkManagerMode.ServerOnly:
				Logger.Info("Changing scene to {@Scene}...", scene.scene);
				NetworkManager.singleton.ServerChangeScene(scene.scene);
				break;
		}
	}

	[ConCommand("asciiart", "Shows Team-Capture ascii art")]
	public static void AsciiArtCmd(string[] args)
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
	public static void SplashMessage(string[] args)
	{
		//Random splash message
		string splashMessagesPath = $"{Game.GetGameExecutePath()}/{SplashScreenResourceFile}";
		if (File.Exists(splashMessagesPath))
		{
			string[] lines = File.ReadAllLines(splashMessagesPath);

			//Select random number
			int index = UnityEngine.Random.Range(0, lines.Length);
			Logger.Info($"	{lines[index]}");
		}
	}
}
