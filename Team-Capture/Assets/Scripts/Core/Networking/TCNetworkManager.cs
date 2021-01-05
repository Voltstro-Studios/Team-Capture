using System;
using System.Collections;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Logging;
using Team_Capture.Core.Networking.Discovery;
using Team_Capture.Core.Networking.Messages;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using Team_Capture.SceneManagement;
using Team_Capture.UI.Panels;
using UnityEngine;
using Voltstro.CommandLineParser;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///     The networking manager for Team-Capture
	/// </summary>
	[RequireComponent(typeof(TCGameDiscovery))]
	internal class TCNetworkManager : NetworkManager
	{
		/// <summary>
		///		Will make the server shutdown when the first connected player disconnects
		/// </summary>
		[CommandLineArgument("closeserveronfirstclientdisconnect")]
		public static bool CloseServerOnFirstClientDisconnect = false;

		/// <summary>
		///     The active <see cref="TCNetworkManager" />
		/// </summary>
		public static TCNetworkManager Instance;

		/// <summary>
		///     The prefab for the <see cref="GameManager" />
		/// </summary>
		[Tooltip("The prefab for the GameManager")] [Header("Team Capture")] [SerializeField]
		private GameObject gameMangerPrefab;

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
		///     The loading screen prefab
		/// </summary>
		[Header("Loading Screen")] [SerializeField]
		private GameObject loadingScreenPrefab;

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

			//Setup loading screen
			//TODO: Handle loading screen stuff somewhere different
			SceneManager.OnBeginSceneLoading += operation => StartCoroutine(OnStartSceneLoadAsync(operation));

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

		#region Loading Screen

		private IEnumerator OnStartSceneLoadAsync(AsyncOperation sceneLoadOperation)
		{
			if (sceneLoadOperation == null)
				yield return null;

			if (singleton.mode == NetworkManagerMode.ServerOnly) yield return null;

			LoadingScreenPanel loadingScreenPanel = Instantiate(loadingScreenPrefab).GetComponent<LoadingScreenPanel>();

			while (!sceneLoadOperation.isDone)
			{
				loadingScreenPanel.SetLoadingBarAmount(Mathf.Clamp01(sceneLoadOperation.progress / .9f));

				yield return null;
			}
		}

		#endregion

		#region Inital Server Join Message

		private void OnReceivedServerConfig(NetworkConnection conn, ServerConfig config)
		{
			//We don't need to listen for the initial server message any more
			NetworkClient.UnregisterHandler<ServerConfig>();

			//Set the game name
			serverConfig = config;
		}

		#endregion

		#region Server Events

		/// <inheritdoc/>
		public override void OnServerSceneChanged(string sceneName)
		{
			Logger.Info("Server changing scene to {@SceneName}", sceneName);

			base.OnServerSceneChanged(sceneName);

			//Instantiate the new game manager
			Instantiate(gameMangerPrefab);
			Logger.Debug("Created GameManager object");

			Logger.Info("Loaded scene to {@SceneName}", sceneName);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//Sent to client the server config
			conn.Send(serverConfig);

			//Create the player object
			GameObject player = Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);

			//Make initial ping
			PingManager.PingClient(conn);

			Logger.Info(
				"Player from {@Address} connected with the connection ID of {@ConnectionID} and a net ID of {@NetID}",
				Transport.activeTransport.ServerGetClientAddress(conn.connectionId), conn.connectionId,
				conn.identity.netId);
		}

		public override void OnStartServer()
		{
			Logger.Info("Starting server...");

			//Set what network address to use, if the computer has multiple adapters then it will default to localhost
			singleton.networkAddress = NetHelper.LocalIpAddress();

			//Start ping service
			PingManager.ServerSetup();

			base.OnStartServer();

			//Start advertising the server when the server starts
			gameDiscovery.AdvertiseServer();

			//Run the server autoexec config
			ConsoleBackend.ExecuteFileCommand(new[] {"server-autoexec"});

			Logger.Info("Server has started and is running on {@Address} with max connections of {@MaxPlayers}!",
				singleton.networkAddress, singleton.maxConnections);

			Server.OnStartServer();
		}

		public override void OnStopServer()
		{
			Logger.Info("Stopping server...");

			base.OnStopServer();

			//Stop advertising the server when the server stops
			gameDiscovery.StopDiscovery();

			PingManager.ServerShutdown();

			Logger.Info("Server stopped!");

			Server.OnStopServer();
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			if(conn == null)
				return;

			Logger.Info("Player {@Id} disconnected from the server.", conn.identity.netId);

			if(CloseServerOnFirstClientDisconnect && conn.identity.netId == 1)
				Game.QuitGame();

			base.OnServerDisconnect(conn);
		}

		#endregion

		#region Client Scene Changing

		public override void OnClientSceneChanged(NetworkConnection conn)
		{
			base.OnClientSceneChanged(conn);

			Logger.Info("The scene has been loaded to {@Scene}", TCScenesManager.GetActiveScene().scene);

			SetupNeededSceneStuffClient();
		}

		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation,
			bool customHandling)
		{
			base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

			if (GameManager.Instance == null)
				return;

			Destroy(GameManager.Instance.gameObject);

			Logger.Info("The server has requested to change the scene to {@NewSceneName}", newSceneName);
		}

		private void SetupNeededSceneStuffClient()
		{
			//Create our the game manager
			Instantiate(gameMangerPrefab);
			Logger.Debug("Created game manager object");
		}

		#endregion

		#region Client Events

		public override void OnClientConnect(NetworkConnection conn)
		{
			//We register for ServerConfigurationMessage, so we get server info
			NetworkClient.RegisterHandler<ServerConfig>(OnReceivedServerConfig);

			base.OnClientConnect(conn);

			Logger.Info("Connected to server {@Address} with the net ID of {@ConnectionId}.", conn.address,
				conn.connectionId);

			//Stop searching for servers
			gameDiscovery.StopDiscovery();
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

			Logger.Info($"Disconnected from server {conn.address}");
		}

		public override void OnStartClient()
		{
			PingManager.ClientSetup();

			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			PingManager.ClientShutdown();

			Logger.Info("Stopped client");
		}

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