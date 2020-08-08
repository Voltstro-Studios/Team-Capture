using Attributes;
using Core;
using Core.Logging;
using Core.Networking;
using Mirror;
using SceneManagement;

public static class UtilCommands
{
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
}
