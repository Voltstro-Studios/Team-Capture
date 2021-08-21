// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Console;
using Team_Capture.SceneManagement;
using Team_Capture.Settings;
using Team_Capture.UI.MOTD;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;
using UniTask = Team_Capture.Integrations.UniTask.UniTask;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///		A class for handling stuff on the client
	/// </summary>
	internal static class Client
	{
		private static TCNetworkManager netManager;
		private static bool clientHasPlayer;

		private static bool clientReceivedConfig;

		internal static ClientStatus Status => status;
		private static ClientStatus status;

		/// <summary>
		///		MOTD mode that the client is using
		/// </summary>
		internal enum ClientMOTDMode
		{
			/// <summary>
			///		The client has disabled MOTDs
			/// </summary>
			Disable,

			/// <summary>
			///		The client only accepts Text only MOTDs
			/// </summary>
			TextOnly,

			/// <summary>
			///		The client accepts both text and web MOTDs
			/// </summary>
			WebSupport
		}

		/// <summary>
		///		Called when the client starts
		/// </summary>
		/// <param name="workingNetManager"></param>
		internal static void OnClientStart(TCNetworkManager workingNetManager)
		{
			status = ClientStatus.Connecting;
			clientReceivedConfig = false;
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
			status = ClientStatus.Offline;
			PingManager.ClientShutdown();
			Logger.Info("Stopped client.");
		}

		/// <summary>
		///		Called when the client connects to a server
		/// </summary>
		/// <param name="conn"></param>
		internal static void OnClientConnect(NetworkConnection conn)
		{
			status = ClientStatus.Connected;
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
			status = ClientStatus.Offline;
			netManager.tcAuthenticator.OnClientDisconnect();
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
			if (GameManager.Instance == null || GameSceneManager.Instance == null)
				return;

			Object.Destroy(GameManager.Instance.gameObject);
			Object.Destroy(GameSceneManager.Instance.gameObject);
			Logger.Info("The server has requested to change the scene to {NewSceneName}", newSceneName);
		}

		/// <summary>
		///		Called after the client changes scenes
		/// </summary>
		internal static void OnClientSceneChanged()
		{
			Object.Instantiate(netManager.gameMangerPrefab);
			Object.Instantiate(netManager.gameSceneManagerPrefab);
			Logger.Info("The scene has been loaded to {Scene}", TCScenesManager.GetActiveScene().scene);

			DisplayMotdAndOrCreatePlayer().Forget();
		}

		private static void OnReceivedServerConfig(ServerConfig config)
		{
			//Server has sent config twice in the same scene session? Probs a modified server
			if (clientHasPlayer)
			{
				Logger.Error("The server has sent it's config twice in the same scene session!");
				return;
			}

			//Set the game name
			netManager.serverConfig = config;
			ClientMotdMode = GameSettings.MultiplayerSettings.MOTDMode;
			clientReceivedConfig = true;
		}

		private static void RequestPlayerObject()
		{
			// Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
			if (!NetworkClient.ready) 
				NetworkClient.Ready();

			NetworkClient.AddPlayer();
			clientHasPlayer = true;

			Logger.Debug("Client has requested player object.");
		}

		private static async UniTaskVoid DisplayMotdAndOrCreatePlayer()
		{
			await UniTask.WaitUntil(() => clientReceivedConfig);

			//If the server has an MOTD, display it before creating a player object
			if (netManager.serverConfig.MotdMode != Server.ServerMOTDMode.Disabled && ClientMotdMode != ClientMOTDMode.Disable)
			{
				if (netManager.serverConfig.MotdMode == Server.ServerMOTDMode.WebOnly && ClientMotdMode == ClientMOTDMode.TextOnly)
				{
					RequestPlayerObject();
					return;
				}

				try
				{
					MOTDUI motdUILogic = Object.Instantiate(netManager.motdUIPrefab).GetComponent<MOTDUI>();
					motdUILogic.Setup(netManager.serverConfig, RequestPlayerObject);
					return;
				}
				catch (InvalidMOTDSettings ex)
				{
					Logger.Error(ex, "Something was wrong with the server's MOTD settings!");
					netManager.StopHost();
					return;
				}
			}

			RequestPlayerObject();
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

		[ConVar("cl_motd", "Set what MOTD mode to use on the client", nameof(UpdateSettings), true)]
		public static ClientMOTDMode ClientMotdMode = ClientMOTDMode.TextOnly;

		#endregion

		[Preserve]
		public static void UpdateSettings()
		{
			GameSettings.MultiplayerSettings.MOTDMode = ClientMotdMode;
			GameSettings.Save();
		}
	}
}