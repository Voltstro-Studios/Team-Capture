using System;
using Mirror;
using Team_Capture.Console;
using Team_Capture.SceneManagement;
using Team_Capture.Settings.Enums;
using Team_Capture.UI.MOTD;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///		A class for handling stuff on the client
	/// </summary>
	internal static class Client
	{
		private static TCNetworkManager netManager;
		private static bool clientHasPlayer;

		/// <summary>
		///		
		/// </summary>
		/// <param name="workingNetManager"></param>
		internal static void OnClientStart(TCNetworkManager workingNetManager)
		{
			clientHasPlayer = false;
			netManager = workingNetManager;

			//We register for ServerConfigurationMessage, so we get server info
			NetworkClient.RegisterHandler<ServerConfig>(OnReceivedServerConfig);

			PingManager.ClientSetup();
			Logger.Info("Started client.");
		}

		/// <summary>
		///		Called when the client is stopped
		/// </summary>
		internal static void OnClientStop()
		{
			PingManager.ClientShutdown();
			Logger.Info("Stopped client.");
		}

		/// <summary>
		///		Called when the client connects to a server
		/// </summary>
		/// <param name="conn"></param>
		internal static void OnClientConnect(NetworkConnection conn)
		{
			Logger.Info("Connected to the server '{Address}' with a connection ID of {ConnectionId}.", conn.address,
				conn.connectionId);

			//Stop searching for servers
			netManager.gameDiscovery.StopDiscovery();
		}

		/// <summary>
		///		Called when the client disconnects from a server
		/// </summary>
		/// <param name="conn"></param>
		internal static void OnClientDisconnect(NetworkConnection conn)
		{
			netManager.StopClient();
			Logger.Info($"Disconnected from server {conn.address}");
		}

		/// <summary>
		///		Called when the client changes scenes
		/// </summary>
		/// <param name="newSceneName"></param>
		internal static void OnClientSceneChanging(string newSceneName)
		{
			clientHasPlayer = false;
			if (GameManager.Instance == null)
				return;

			Object.Destroy(GameManager.Instance.gameObject);
			Logger.Info("The server has requested to change the scene to {@NewSceneName}", newSceneName);
		}

		/// <summary>
		///		Called after the client changes scenes
		/// </summary>
		internal static void OnClientSceneChanged(NetworkConnection conn)
		{
			Object.Instantiate(netManager.gameMangerPrefab);
			Logger.Info("The scene has been loaded to {Scene}", TCScenesManager.GetActiveScene().scene);
		}

		private static void OnReceivedServerConfig(NetworkConnection conn, ServerConfig config)
		{
			//Server has sent config twice in the same scene session? Probs a modified server
			if (clientHasPlayer)
			{
				Logger.Error("The server has sent it's config twice in the same scene session!");
				return;
			}

			//Set the game name
			netManager.serverConfig = config;

			//If the server has an MOTD, display it before creating a player object
			if (config.motdMode != Server.ServerMOTDMode.Disabled)
			{
				try
				{
					MOTDUI motdUILogic = Object.Instantiate(netManager.motdUIPrefab).GetComponent<MOTDUI>();
					motdUILogic.Setup(config, () => RequestPlayerObject(conn));
					return;
				}
				catch (InvalidMOTDSettings ex)
				{
					Logger.Error(ex, "Something was wrong with the server's MOTD settings!");
					netManager.StopHost();
				}
			}

			RequestPlayerObject(conn);
		}

		private static void RequestPlayerObject(NetworkConnection conn)
		{
			// Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
			if (!ClientScene.ready) 
				ClientScene.Ready(conn);

			ClientScene.AddPlayer(conn);
			clientHasPlayer = true;

			Logger.Debug("Client has requested player object.");
		}

		#region Console Commands

		[ConCommand("connect", "Connects to a server", CommandRunPermission.ClientOnly, 1, 1)]
		public static void ConnectCommand(string[] args)
		{
			try
			{
				NetworkManager networkManager = NetworkManager.singleton;
				networkManager.StopClient();

				networkManager.networkAddress = args[0];
				networkManager.StartClient();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "An error occurred while connecting to the server '{Address}'", args[0]);
			}
		}

		#endregion
	}
}