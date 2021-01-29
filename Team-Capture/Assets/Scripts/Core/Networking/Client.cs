using Mirror;
using Team_Capture.SceneManagement;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	internal static class Client
	{
		private static TCNetworkManager netManager;

		internal static void OnClientStart(TCNetworkManager workingNetManager)
		{
			netManager = workingNetManager;

			PingManager.ClientSetup();
			Logger.Info("Started client.");
		}

		internal static void OnClientStop()
		{
			PingManager.ClientShutdown();
			Logger.Info("Stopped client.");
		}

		internal static void OnClientConnect(NetworkConnection conn)
		{
			//We register for ServerConfigurationMessage, so we get server info
			NetworkClient.RegisterHandler<ServerConfig>(OnReceivedServerConfig);

			if (!netManager.clientLoadedScene)
			{
				// Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
				if (!ClientScene.ready) 
					ClientScene.Ready(conn);
				ClientScene.AddPlayer(conn);
			}

			Logger.Info("Connected to the server '{Address}' with a connection ID of {ConnectionId}.", conn.address,
				conn.connectionId);

			//Stop searching for servers
			netManager.gameDiscovery.StopDiscovery();
		}

		internal static void OnClientDisconnect(NetworkConnection conn)
		{
			netManager.StopClient();
			Logger.Info($"Disconnected from server {conn.address}");
		}

		internal static void OnClientSceneChanging(string newSceneName)
		{
			if (GameManager.Instance == null)
				return;

			Object.Destroy(GameManager.Instance.gameObject);
			Logger.Info("The server has requested to change the scene to {@NewSceneName}", newSceneName);
		}

		internal static void OnClientSceneChanged()
		{
			Object.Instantiate(netManager.gameMangerPrefab);
			Logger.Info("The scene has been loaded to {Scene}", TCScenesManager.GetActiveScene().scene);
		}

		private static void OnReceivedServerConfig(NetworkConnection conn, ServerConfig config)
		{
			//We don't need to listen for the initial server message any more
			NetworkClient.UnregisterHandler<ServerConfig>();

			//Set the game name
			netManager.serverConfig = config;
		}
	}
}