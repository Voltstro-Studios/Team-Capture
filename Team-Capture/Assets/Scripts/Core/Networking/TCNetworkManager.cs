using System;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Networking.Discovery;
using Team_Capture.LagCompensation;
using Team_Capture.Logging;
using UnityEngine;
using Voltstro.CommandLineParser;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///     The networking manager for Team-Capture
	/// </summary>
	[RequireComponent(typeof(TCGameDiscovery))]
	internal class TCNetworkManager : NetworkManager
	{
		/// <summary>
		///     The active <see cref="TCNetworkManager" />
		/// </summary>
		public static TCNetworkManager Instance;

		/// <summary>
		///     The prefab for the <see cref="GameManager" />
		/// </summary>
		[Tooltip("The prefab for the GameManager")] [Header("Team Capture")]
		public GameObject gameMangerPrefab;

		/// <summary>
		///     How many frames to keep
		/// </summary>
		public int maxFrameCount = 128;

		/// <summary>
		///     The active <see cref="TCGameDiscovery" />
		/// </summary>
		[HideInInspector] public TCGameDiscovery gameDiscovery;

		/// <summary>
		///     The config for the server
		/// </summary>
		public ServerConfig serverConfig;

		/// <summary>
		///		Are we a server or not
		/// </summary>
		public static bool IsServer => Instance.mode == NetworkManagerMode.ServerOnly;

		public override void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			//Replace Mirror's logger with ours
			LogFactory.ReplaceLogHandler(new MirrorLogHandler());

			base.Awake();

			Instance = this;
		}

		public override void Start()
		{
			//Setup configuration with our launch arguments
			serverConfig.gameName = GameName;
			singleton.maxConnections = MaxPlayers;
			singleton.onlineScene = Scene;

			//We are running in headless mode
			if (Game.IsHeadless)
				//Start the server
				StartServer();

			//TODO: Make auth movement not server framerate dependent
			Application.targetFrameRate = 128;
		}

		public void Update()
		{
			if (mode == NetworkManagerMode.ServerOnly) PingManager.ServerPingUpdate();
		}

		public void FixedUpdate()
		{
			//If we are the server then update simulated objects
			if (mode == NetworkManagerMode.ServerOnly) SimulationHelper.UpdateSimulationObjectData();
		}

		#region Server Events

		public override void OnStartServer() 
			=> Server.OnStartServer(this);

		public override void OnStopServer()
			=> Server.OnStopServer();

		public override void OnServerConnect(NetworkConnection conn) 
			=> Server.OnServerAddClient(conn);

		public override void OnServerDisconnect(NetworkConnection conn)
			=> Server.OnServerRemoveClient(conn);

		public override void OnServerAddPlayer(NetworkConnection conn) 
			=> Server.ServerCreatePlayerObject(conn, playerPrefab);

		public override void OnServerChangeScene(string newSceneName)
			=> Server.OnServerSceneChanging(newSceneName);

		public override void OnServerSceneChanged(string sceneName) 
			=> Server.OnServerChangedScene(sceneName);


		#endregion

		#region Client Events

		public override void OnStartClient()
			=> Client.OnClientStart(this);

		public override void OnStopClient()
			=> Client.OnClientStop();

		public override void OnClientConnect(NetworkConnection conn)
			=> Client.OnClientConnect(conn);

		public override void OnClientDisconnect(NetworkConnection conn)
			=> Client.OnClientDisconnect(conn);

		public override void OnClientSceneChanged(NetworkConnection conn)
			=> Client.OnClientSceneChanged(conn);

		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation,
			bool customHandling)
			=> Client.OnClientSceneChanging(newSceneName);

		#endregion

		#region Console Commands

		[ConCommand("connect", "Connects to a server", CommandRunPermission.ClientOnly, 1, 1)]
		public static void ConnectCommand(string[] args)
		{
			try
			{
				singleton.StopHost();

				singleton.networkAddress = args[0];
				singleton.StartClient();
			}
			catch (Exception e)
			{
				Logger.Error("An error occured: {@Error}", e);
			}
		}

		[ConCommand("disconnect", "Disconnect from the current game")]
		public static void DisconnectCommand(string[] args)
		{
			if (singleton.mode == NetworkManagerMode.Offline)
			{
				Logger.Error("You are not in a game!");
				return;
			}

			singleton.StopHost();
		}

		[ConCommand("startserver", "Starts a server", CommandRunPermission.ClientOnly, 1, 1)]
		public static void StartServerCommand(string[] args)
		{
			string scene = args[0];
			singleton.onlineScene = scene;

			singleton.StartServer();
		}

		[ConCommand("gamename", "Sets the game name", CommandRunPermission.ServerOnly)]
		public static void SetGameNameCommand(string[] args)
		{
			Instance.serverConfig.gameName = string.Join(" ", args);
			Logger.Info("Game name was set to {@Name}", Instance.serverConfig.gameName);
		}

		[ConCommand("sv_address", "Sets the server's address", CommandRunPermission.ServerOnly, 1, 1)]
		public static void SetAddressCommand(string[] args)
		{
			if (singleton == null) return;

			singleton.networkAddress = args[0];
			Logger.Info("Server's address was set to {@Address}", args[0]);
		}

		#endregion

		#region Command Line Arguments

		[CommandLineArgument("gamename")] public static string GameName = "Team-Capture Game";

		[CommandLineArgument("maxplayers")] public static int MaxPlayers = 16;

		[CommandLineArgument("scene")] public static string Scene = "dm_ditch";

		#endregion
	}
}