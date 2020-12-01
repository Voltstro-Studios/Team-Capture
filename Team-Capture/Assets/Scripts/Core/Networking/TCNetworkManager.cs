using System;
using System.Collections;
using Console;
using Core.Logging;
using Core.Networking.Discovery;
using Core.Networking.Messages;
using Helper;
using LagCompensation;
using Mirror;
using Pickups;
using SceneManagement;
using UI.Panels;
using UnityEngine;
using Voltstro.CommandLineParser;
using Logger = Core.Logging.Logger;

namespace Core.Networking
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
		public ServerConfig serverConfig = new ServerConfig();

		/// <summary>
		///     The loading screen prefab
		/// </summary>
		[Header("Loading Screen")] [SerializeField]
		private GameObject loadingScreenPrefab;

		public override void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			LogFactory.ReplaceLogHandler(new MirrorLogHandler());

			base.Awake();

			Instance = this;
		}

		public override void Start()
		{
			serverConfig.gameName = GameName;
			singleton.maxConnections = MaxPlayers;
			singleton.onlineScene = Scene;

			//We are running in headless mode
			if (Game.IsHeadless)
				//Start the server
				StartServer();

			SceneManager.OnBeginSceneLoading += operation => StartCoroutine(OnStartSceneLoadAsync(operation));
			Application.targetFrameRate = 128;
		}

		public void FixedUpdate()
		{
			//If we are the server then update simulated objects
			if (mode == NetworkManagerMode.ServerOnly) SimulationHelper.UpdateSimulationObjectData();
		}

		#region Loading Screen

		private IEnumerator OnStartSceneLoadAsync(AsyncOperation sceneLoadOperation)
		{
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

		private void OnServerJoinMessage(NetworkConnection conn, InitialClientJoinMessage message)
		{
			//We don't need to listen for the initial server message any more
			NetworkClient.UnregisterHandler<InitialClientJoinMessage>();

			//Set the game name
			serverConfig = message.ServerConfig;
		}

		#endregion

		private void SetupNeededSceneStuffClient()
		{
			//Create our the game manager
			Instantiate(gameMangerPrefab);
			Logger.Debug("Created game manager object");
		}

		#region Console Arguments

		[CommandLineArgument("gamename")] public static string GameName = "Team-Capture Game";

		[CommandLineArgument("maxplayers")] public static int MaxPlayers = 16;

		[CommandLineArgument("scene")] public static string Scene = "dm_ditch";

		#endregion

		#region Server Events

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
			//Sent to client info about the server
			//TODO: Do this in an authenticator
			conn.Send(new InitialClientJoinMessage
			{
				ServerConfig = serverConfig
			});

			//Send pickup's status
			ServerPickupManager.OnClientJoined(conn);

			//Create the player object
			GameObject player = Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);

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
			if (Transport.activeTransport is IgnoranceThreaded threaded)
				threaded.ServerBindAddress = singleton.networkAddress;

			//Setup the pickup manager
			ServerPickupManager.Setup();

			base.OnStartServer();

			//Start advertising the server when the server starts
			gameDiscovery.AdvertiseServer();

			//Run the server autoexec config
			ConsoleBackend.ExecuteFileCommand(new[] {"server-autoexec"});

			Logger.Info("Server has started and is running on {@Address} with max connections of {@MaxPlayers}!",
				singleton.networkAddress, singleton.maxConnections);
		}

		public override void OnStopServer()
		{
			Logger.Info("Stopping server...");

			base.OnStopServer();

			ServerPickupManager.Shutdown();

			//Stop advertising the server when the server stops
			gameDiscovery.StopDiscovery();

			Logger.Info("Server stopped!");
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			Logger.Info("Player {@Id} disconnected from the server.", conn.identity.netId);

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

		#endregion

		#region Client Events

		public override void OnClientConnect(NetworkConnection conn)
		{
			//We register for InitialClientJoinMessage, so we get server info
			//TODO: Should do this in an authenticator 
			NetworkClient.RegisterHandler<InitialClientJoinMessage>(OnServerJoinMessage);

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
			ClientPickupManager.Setup();

			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			ClientPickupManager.Shutdown();

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
	}
}