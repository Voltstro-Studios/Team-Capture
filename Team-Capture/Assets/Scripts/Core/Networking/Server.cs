using System;
using System.Diagnostics;
using System.IO;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using UnityEngine;
using Voltstro.CommandLineParser;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///		A class for handling stuff on the server
	/// </summary>
	internal static class Server
	{
		/// <summary>
		///		Will make the server shutdown when the first connected player disconnects
		/// </summary>
		[CommandLineArgument("closeserveronfirstclientdisconnect")]
		public static bool CloseServerOnFirstClientDisconnect = false;

		/// <summary>
		///		The timeout time to wait while creating a server from the UI
		/// </summary>
		[ConVar("sv_serverstarttimeout", "The timeout time to wait while creating a server from the UI")]
		private static int TimeOutServerTime = 6;

		//MOTD related Variables

		/// <summary>
		///		The base path to the MOTD
		/// </summary>
		private const string MotdPath = "/Resources/motd.txt";

		/// <summary>
		///		Whats the default text in the MOTD
		/// </summary>
		private const string MotdDefaultText = "<style=\"Title\">Welcome to Team-Capture!</style>\n\n" +
		                                       "<style=\"h2\">Map Rotation</style>\n" +
		                                       "Here is our map rotation:\n" +
		                                       "    dm_ditch\n\n" +
		                                       "<style=\"h2\">Rules</style>\n" +
		                                       "    - No cheating\n" +
		                                       "    - Have fun!";

		//Server online file related variables

		/// <summary>
		///		Server online file name
		/// </summary>
		private const string ServerOnlineFile = "SERVERONLINE";

		/// <summary>
		///		The message to put in the server online file (Done as a joke)
		/// </summary>
		private static readonly byte[] ServerOnlineFileMessage = {65, 32, 45, 71, 97, 119, 114, 32, 71, 117, 114, 97};

		/// <summary>
		///		Runtime path to the server online file
		/// </summary>
		private static string serverOnlineFilePath;

		/// <summary>
		///		The <see cref="FileStream"/> for the server online file
		/// </summary>
		private static FileStream serverOnlineFileStream;

		/// <summary>
		///		Who was the first connected ID
		///		<para>KCP just seems to assign random connection IDs, so we just hope the first person never has <see cref="int.MaxValue"/> as their ID</para>
		/// </summary>
		private static int firstConnectionId = int.MaxValue;

		/// <summary>
		///		The working <see cref="TCNetworkManager"/>, set on <see cref="OnStartServer"/>
		/// </summary>
		private static TCNetworkManager netManager;

		/// <summary>
		///		MOTD mode that a server is using
		/// </summary>
		internal enum ServerMOTDMode : byte
		{
			/// <summary>
			///		The server's MOTD is disabled
			/// </summary>
			Disabled,

			/// <summary>
			///		The server only has a text based MOTD
			/// </summary>
			TextOnly,

			/// <summary>
			///		The server only has a web based MOTD
			/// </summary>
			WebOnly,

			/// <summary>
			///		The server supports both web and text MOTDs
			/// </summary>
			WebWithTextBackup
		}

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

			SetupServerConfig();

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
				Logger.Error(ex, "An error occurred while setting up the server!");
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
				Logger.Error(ex, "An error occurred while shutting down the server!");
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
			//Sent to client the server config
			conn.Send(TCNetworkManager.Instance.serverConfig);

			//Lets just hope our transport never assigns the first connection max value of int
			if (CloseServerOnFirstClientDisconnect && firstConnectionId == int.MaxValue)
				firstConnectionId = conn.connectionId;

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

			//Our first connected client disconnected
			if(CloseServerOnFirstClientDisconnect && conn.connectionId == firstConnectionId)
				Game.QuitGame();
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

			NetworkServer.SendToAll(TCNetworkManager.Instance.serverConfig);

			Logger.Info("Server changed scene to {SceneName}", sceneName);
		}

		/// <summary>
		///		Called when a client request for a player object
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="playerPrefab"></param>
		internal static void ServerCreatePlayerObject(NetworkConnection conn, GameObject playerPrefab)
		{
			//Create the player object
			GameObject player = Object.Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);

			//Make initial ping
			PingManager.PingClient(conn);

			Logger.Info("Created player object for {NetID}", conn.identity.netId);
		}

		///  <summary>
		/// 		Creates a new server process and connects this process to it
		///  </summary>
		///  <param name="workingNetManager"></param>
		///  <param name="gameName"></param>
		///  <param name="sceneName"></param>
		///  <param name="maxPlayers"></param>
		///  <param name="onFailedToStart"></param>
		internal static void CreateServerAndConnectToServer(this NetworkManager workingNetManager, 
			string gameName, string sceneName, int maxPlayers,
			Action onFailedToStart = null)
		{
#if UNITY_EDITOR
			string serverOnlinePath =
				$"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/{ServerOnlineFile}";
#else
			string serverOnlinePath = $"{Game.GetGameExecutePath()}/{ServerOnlineFile}";
#endif

			//Check to make sure the server online file doesn't already exist
			if (File.Exists(serverOnlinePath))
			{
				Logger.Error("A server is already running!");
				return;
			}

			//Create and start the process
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

			//We need to wait for the server online file, and to not cause the game to freeze we run it async
			WaitForServerOneFileAndConnect(serverOnlinePath, () =>
			{
				//Start the client
				workingNetManager.networkAddress = "localhost";
				workingNetManager.StartClient();
			}, onFailedToStart).Forget();
		}

		private static UniTask WaitForServerOneFileAndConnect(string serverOnlinePath, 
			Action onSuccessFullCompletion = null,
			Action onFailToStart = null)
		{
			float timeUntilCancel = Time.time + TimeOutServerTime;

			//Wait until the server online file exist
			while (!File.Exists(serverOnlinePath))
			{
				//TODO: We should have a delay here, so the CPU doesn't get entirely used just to constantly check if the file exists or not, but for some reason UniTask.Delay stopped working...
				//If we hit the timeout time, then fail it
				if (Time.time >= timeUntilCancel)
				{
					Logger.Error("Server process did not start for some reason! Not connecting.");
					onFailToStart?.Invoke();
					return UniTask.CompletedTask;
				}
			}

			onSuccessFullCompletion?.Invoke();
			return UniTask.CompletedTask;
		}

		private static void SetupServerConfig()
		{
			//Setup configuration with our launch arguments
			netManager.serverConfig.gameName = GameName;
			netManager.serverConfig.motdMode = ServerMotdMode;
			netManager.serverConfig.motdUrl = ServerMotdUrl;
			netManager.maxConnections = MaxPlayers;
			netManager.onlineScene = Scene;

			//Setup MOTD
			string gamePath = Game.GetGameExecutePath();

			if (netManager.serverConfig.motdMode == ServerMOTDMode.TextOnly || netManager.serverConfig.motdMode == ServerMOTDMode.WebWithTextBackup)
			{
				string motdGamePath = $"{gamePath}{MotdPath}";
				string motdData;

				//If the MOTD file doesn't exist, create it
				if (!File.Exists(motdGamePath))
				{
					WriteDefaultMotd(motdGamePath);
					motdData = MotdDefaultText;
				}
				else //The file exists
				{
					motdData = File.ReadAllText(motdGamePath);
					if (string.IsNullOrWhiteSpace(motdData))
					{
						WriteDefaultMotd(motdGamePath);
						motdData = MotdDefaultText;
					}
				}

				//Check to make sure the MOTD text isn't beyond what is allowed to be sent over
				//(As of writing this its 32,768, sooo probs long enough for anyone lol)
				if (motdData.Length == NetworkWriter.MaxStringLength)
				{
					Logger.Error("The MOTD text is longer then the max allowed text length! ({MaxStringLength})", NetworkWriter.MaxStringLength);
					return;
				}

				netManager.serverConfig.motdText = motdData;
			}
		}

		private static void WriteDefaultMotd(string motdPath)
		{
			Logger.Warn("Created new default MOTD.");
			File.WriteAllText(motdPath, MotdDefaultText);
		}

		#region Console Commands

		[ConCommand("startserver", "Starts a server", CommandRunPermission.ClientOnly, 1, 1)]
		public static void StartServerCommand(string[] args)
		{
			NetworkManager networkManager = NetworkManager.singleton;
			string scene = args[0];
			networkManager.onlineScene = scene;

			networkManager.StartServer();
		}

		[ConCommand("gamename", "Sets the game name", CommandRunPermission.ServerOnly)]
		public static void SetGameNameCommand(string[] args)
		{
			TCNetworkManager.Instance.serverConfig.gameName = string.Join(" ", args);
			Logger.Info("Game name was set to {Name}", TCNetworkManager.Instance.serverConfig.gameName);
		}

		[ConCommand("sv_address", "Sets the server's address", CommandRunPermission.ServerOnly, 1, 1)]
		public static void SetAddressCommand(string[] args)
		{
			NetworkManager.singleton.networkAddress = args[0];
			Logger.Info("Server's address was set to {Address}", args[0]);
		}

		[ConVar("sv_gamename", "Sets the game name")]
		[CommandLineArgument("gamename")] 
		public static string GameName = "Team-Capture Game";

		[ConVar("sv_maxplayers", "How many players do we support")]
		[CommandLineArgument("maxplayers")] 
		public static int MaxPlayers = 16;

		[ConVar("sv_scene", "Sets what scene to use on the server")]
		[CommandLineArgument("scene")] 
		public static string Scene = "dm_ditch";

		[ConVar("sv_motd_mode", "Set what MOTD mode to use on the server")] 
		public static ServerMOTDMode ServerMotdMode = ServerMOTDMode.TextOnly;

		[ConVar("sv_motd_url", "Sets what URL for the MOTD to use")] 
		public static string ServerMotdUrl = "https://voltstro.dev";

		#endregion
	}
}