using System;
using Core.Console;
using Core.Logger;
using Core.Networking;
using Mirror;
using SceneManagement;

public static class UtilCommands
{
	[ConCommand("scene", "Loads a scene")]
	public static void LoadScene(string[] args)
	{
		if (args.Length != 1)
		{
			Logger.Log("Invalid arguments!");
			return;
		}

		if (TCNetworkManager.Instance != null && TCNetworkManager.Instance.mode != NetworkManagerMode.Offline)
		{
			Logger.Log("Cannot change the scene while connected to a server!", LogVerbosity.Error);
			return;
		}

		TCScene scene = TCScenesManager.FindSceneInfo(args[0]);
		if (scene == null)
		{
			Logger.Log($"The scene '{args[0]}' doesn't exist!");
			return;
		}

		if (!scene.canLoadTo)
		{
			Logger.Log("You cannot load to this scene!");
			return;
		}

		TCScenesManager.LoadScene(scene);
	}

	[ConCommand("connect", "Connects to a server")]
	public static void ConnectCommand(string[] args)
	{
		if (args.Length != 1)
		{
			Logger.Log("Invalid arguments!", LogVerbosity.Error);
			return;
		}

		try
		{
			NetworkManager.singleton.StopHost();
			NetworkManager.singleton.StartClient(new Uri(args[0]));
		}
		catch (Exception e)
		{
			Logger.Log(e.Message, LogVerbosity.Error);
		}
	}
}
