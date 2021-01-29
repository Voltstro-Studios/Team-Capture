using System;
using System.Diagnostics;
using System.IO;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

namespace Team_Capture.Core.Networking
{
	public static class Server
	{
		private const string ServerOnlineFile = "SERVERONLINE";
		private static readonly byte[] ServerOnlineFileMessage = {65, 32, 45, 71, 97, 119, 114, 32, 71, 117, 114, 97};

		private static string serverOnlineFilePath;
		private static FileStream serverOnlineFileStream;
		private static TCNetworkManager netManager;

		/// <summary>
		///		Call this when the server is started
		/// </summary>
		internal static void OnStartServer(TCNetworkManager workingNetManager)
		{
			serverOnlineFilePath = $"{Game.GetGameExecutePath()}/{ServerOnlineFile}";

			if (File.Exists(serverOnlineFilePath))
				throw new Exception("Server is already online!");

			netManager = workingNetManager;

			Logger.Info("Starting server...");

			//Set what network address to use and start to advertise the server on lan
			netManager.networkAddress = NetHelper.LocalIpAddress();
			netManager.gameDiscovery.AdvertiseServer();

			//Start ping service
			PingManager.ServerSetup();

			//Run the server autoexec config
			ConsoleBackend.ExecuteFile("server-autoexec");

			//Create server online file
			try
			{
				serverOnlineFileStream = File.Create(serverOnlineFilePath, 128, FileOptions.DeleteOnClose);
				serverOnlineFileStream.Write(ServerOnlineFileMessage, 0, ServerOnlineFileMessage.Length);
				serverOnlineFileStream.Flush();
				File.SetAttributes(serverOnlineFilePath, FileAttributes.Hidden);
			}
			catch (IOException ex)
			{
				Logger.Error(ex, "An error occured while setting up the server!");
				netManager.StopHost();

				return;
			}

			Logger.Info("Server has started and is running on '{Address}' with max connections of {MaxPlayers}!",
				netManager.networkAddress, netManager.maxConnections);
		}

		/// <summary>
		///		Call this when the server is stopped
		/// </summary>
		internal static void OnStopServer()
		{
			Logger.Info("Stopping server...");
			PingManager.ServerShutdown();

			//Stop advertising the server when the server stops
			netManager.gameDiscovery.StopDiscovery();

			Logger.Info("Server stopped!");

			//Close server online file stream
			try
			{
				serverOnlineFileStream.Close();
				serverOnlineFileStream.Dispose();
				serverOnlineFileStream = null;
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "An error occured while shutting down the server!");
			}
			
			netManager = null;

			//Double check that the file is deleted
			if(File.Exists(serverOnlineFilePath))
				File.Delete(serverOnlineFilePath);

		}

		/// <summary>
		///		Call when a client connects
		/// </summary>
		/// <param name="conn"></param>
		internal static void OnServerAddClient(NetworkConnection conn)
		{
			Logger.Info(
				"Client from '{Address}' connected with the connection ID of {ConnectionID}.",
				conn.address, conn.connectionId);
		}

		/// <summary>
		///		Call when a client disconnects
		/// </summary>
		/// <param name="conn"></param>
		internal static void OnServerRemoveClient(NetworkConnection conn)
		{
			NetworkServer.DestroyPlayerForConnection(conn);
			Logger.Info("Client '{ConnectionId}' disconnected from the server.", conn.connectionId);

			//TODO: Fix this
			/*
			if(CloseServerOnFirstClientDisconnect && conn.identity.netId == 1)
				Game.QuitGame();
			*/
		}

		/// <summary>
		///		Called when a scene is about to be changed
		/// </summary>
		/// <param name="sceneName"></param>
		internal static void OnServerSceneChanging(string sceneName)
		{
			Logger.Info("Server is changing scene to {SceneName}...", sceneName);
		}

		/// <summary>
		///		Called after the scene changes
		/// </summary>
		/// <param name="sceneName"></param>
		internal static void OnServerChangedScene(string sceneName)
		{
			//Instantiate the new game manager
			Object.Instantiate(netManager.gameMangerPrefab);
			Logger.Debug("Created GameManager object");

			Logger.Info("Server changed scene to {SceneName}", sceneName);
		}

		/// <summary>
		///		Called when a client request for a player object
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="playerPrefab"></param>
		internal static void ServerCreatePlayerObject(NetworkConnection conn, GameObject playerPrefab)
		{
			//Sent to client the server config
			conn.Send(TCNetworkManager.Instance.serverConfig);

			//Create the player object
			GameObject player = Object.Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);

			//Make initial ping
			PingManager.PingClient(conn);

			Logger.Info("Created player object for {NetID}", conn.identity.netId);
		}

		public static void CreateServerAndConnectToServer(this NetworkManager workingNetManager, string gameName, string sceneName, int maxPlayers)
		{
#if UNITY_EDITOR
			string serverOnlinePath =
				$"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/{ServerOnlineFile}";
#else
			string serverOnlinePath = $"{Game.GetGameExecutePath()}/{ServerOnlineFile}";
#endif

			if (File.Exists(serverOnlinePath))
			{
				Logger.Error("A server is already running!");
				return;
			}

			Process newTcServer = new Process
			{
				StartInfo = new ProcessStartInfo
				{
#if UNITY_EDITOR
					FileName =
						$"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/Team-Capture.exe",
#elif UNITY_STANDALONE_WIN
					FileName = "Team-Capture.exe",
#else
					FileName = "Team-Capture",
#endif
					Arguments =
						$"-batchmode -nographics -gamename \"{gameName}\" -scene {sceneName} -maxplayers {maxPlayers} -closeserveronfirstclientdisconnect"
				}
			};
			newTcServer.Start();

			while (!File.Exists(serverOnlinePath))
			{
			}

			workingNetManager.networkAddress = "localhost";
			workingNetManager.StartClient();
		}
	}
}