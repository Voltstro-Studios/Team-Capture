using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Networking.Discovery;
using Team_Capture.LagCompensation;
using Team_Capture.Logging;
using UnityEngine;
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

		public GameObject motdUIPrefab;

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

		[ConCommand("stop", "Stops the current game, whether that is disconnecting or stopping the server")]
		public static void DisconnectCommand(string[] args)
		{
			NetworkManager networkManager = singleton;
			if (networkManager.mode == NetworkManagerMode.Offline)
			{
				Logger.Error("You are not in a game!");
				return;
			}

			networkManager.StopHost();

			//Quit the game if we are headless
			if(Game.IsHeadless)
				Game.QuitGame();
		}
	}
}