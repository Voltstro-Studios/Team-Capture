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
		if (TCNetworkManager.Instance != null && TCNetworkManager.Instance.mode != NetworkManagerMode.Offline)
		{
			Logger.Error("Cannot change the scene while connected to a server!");
			return;
		}

		TCScene scene = TCScenesManager.FindSceneInfo(args[0]);
		if (scene == null)
		{
			Logger.Error($"The scene '{args[0]}' doesn't exist!");
			return;
		}

		if (!scene.canLoadTo)
		{
			Logger.Error("You cannot load to this scene!");
			return;
		}

		TCScenesManager.LoadScene(scene);
	}
}
