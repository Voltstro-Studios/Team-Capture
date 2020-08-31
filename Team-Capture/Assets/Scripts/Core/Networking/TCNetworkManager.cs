using System;
using System.Collections;
using Attributes;
using Console;
using Core.Networking.Discovery;
using Core.Networking.Messages;
using LagCompensation;
using Mirror;
using Pickups;
using Player;
using SceneManagement;
using UI.Panels;
using UnityEngine;
using Weapons;
using Logger = Core.Logging.Logger;

namespace Core.Networking
{
	[RequireComponent(typeof(TCGameDiscovery))]
	public class TCNetworkManager : NetworkManager
	{
		/// <summary>
		/// The active <see cref="TCNetworkManager"/>
		/// </summary>
		public static TCNetworkManager Instance;

		[Header("Team Capture")] 
		[SerializeField] private GameObject gameMangerPrefab;

		[SerializeField] private string pickupTagName = "Pickup";

		[SerializeField] private float playerLatencyUpdateTime = 2.0f;

		public int maxFrameCount = 128;
		public TCWeapon[] stockWeapons;

		[HideInInspector] public TCGameDiscovery gameDiscovery;
		public string gameName;

		[Header("Loading Screen")]
		[SerializeField] private GameObject loadingScreenPrefab;
		private LoadingScreenPanel loadingScreenPanel;

		public override void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			base.Awake();

			Instance = this;
		}

		public override void Start()
		{
			//Set scene
			if (CommandLineParser.Options.TryGetValue("-scene", out string scene))
			{
				if (scene != null)
				{
					if (TCScenesManager.FindSceneInfo(scene) != null)
						onlineScene = scene;
					else
						Logger.Error("The scene '{@Scene}' doesn't exist!", scene);
				}
			}

			//Set server name
			if (CommandLineParser.Options.TryGetValue("-name", out string serverName))
			{
				if (serverName != null)
					gameName = serverName;
			}

			//Max players
			if (CommandLineParser.Options.TryGetValue("-maxplayers", out string maxPlayers))
			{
				if (int.TryParse(maxPlayers, out int intPlayerPlayers))
					singleton.maxConnections = intPlayerPlayers;
				else
					Logger.Error("The max players argument isn't just a number!");
			}

			//We are running in headless mode
			if (Game.IsHeadless)
			{
				//Start the server
				StartServer();
			}
			else
			{
				//Setup loading events
				TCScenesManager.PreparingSceneLoadEvent += OnPreparingSceneLoad;
				TCScenesManager.StartSceneLoadEvent += StartSceneLoad;
			}

			Application.targetFrameRate = 128;
			Time.fixedDeltaTime = 1 / 60f;
		}

		public void FixedUpdate()
		{
			//If we are the server then update simulated objects
			if (mode == NetworkManagerMode.ServerOnly)
			{
				SimulationHelper.UpdateSimulationObjectData();
			}
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			//Clear the pickup list
			ServerPickupManager.ClearUnActivePickupsList();

			Logger.Info("Server changing scene to {@SceneName}", sceneName);

			base.OnServerSceneChanged(sceneName);

			//Instantiate the new game manager
			Instantiate(gameMangerPrefab);
			Logger.Debug("Created GameManager object");

			//Setup pickups
			//TODO: This should be done in the server pickup manager
			//TODO: We should save all references to pickups to the associated scene file
			GameObject[] pickups = GameObject.FindGameObjectsWithTag(pickupTagName);
			foreach (GameObject pickup in pickups)
			{
				//Make sure it has the Pickup script on it
				Pickup pickupLogic = pickup.GetComponent<Pickup>();
				if(pickupLogic == null)
				{
					Logger.Error("The pickup with the name of `{@PickupName}` @ {@PickupTransform} doesn't have the {@Pickup} behaviour on it!", pickup.name, pickup.transform, typeof(Pickup));
					continue;
				}

				//Setup the trigger
				pickupLogic.SetupTrigger();
			}

			Logger.Info("Loaded scene to {@SceneName}", sceneName);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//Sent to client info about the server
			conn.Send(new InitialClientJoinMessage
			{
				GameName = gameName,
				DeactivatedPickups = ServerPickupManager.GetUnActivePickups()
			});

			//Create the player object
			GameObject player = Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);

			Logger.Info("Player from {@Address} connected with the connection ID of {@ConnectionID} and a net ID of {@NetID}", conn.address, conn.connectionId, conn.identity.netId);
		}

		public override void OnStartServer()
		{
			Logger.Info("Starting server...");

			base.OnStartServer();

			//Start advertising the server when the server starts
			gameDiscovery.AdvertiseServer();

			StartCoroutine(UpdateLatency());

			//Run the server autoexec config
			ConsoleBackend.ExecuteFile(new []{"server-autoexec"});

			Logger.Info("Server has started and is running on {@Address} with max connections of {@MaxPlayers}!", singleton.networkAddress, singleton.maxConnections);
		}

		public override void OnStopServer()
		{
			Logger.Info("Stopping server...");

			StopCoroutine(UpdateLatency());

			base.OnStopServer();

			//Stop advertising the server when the server stops
			gameDiscovery.StopDiscovery();

			Logger.Info("Server stopped!");
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			//We register for InitialClientJoinMessage, so we get server info
			NetworkClient.RegisterHandler<InitialClientJoinMessage>(OnServerJoinMessage);

			base.OnClientConnect(conn);

			Logger.Info("Connected to server {@Address} with the net ID of {@ConnectionId}.", conn.address, conn.connectionId);

			//Stop searching for servers
			gameDiscovery.StopDiscovery();
		}

		public override void OnClientSceneChanged(NetworkConnection conn)
		{
			base.OnClientSceneChanged(conn);

			Logger.Info("The scene has been loaded to {@Scene}", TCScenesManager.GetActiveScene().scene);

			SetupNeededSceneStuffClient();
		}

		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
		{
			base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

			if(GameManager.Instance == null)
				return;

			Destroy(GameManager.Instance.gameObject);

			Logger.Info("The server has requested to change the scene to {@NewSceneName}", newSceneName);
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			base.OnServerDisconnect(conn);

			Logger.Info("Player from {@Address} disconnected.", conn.address);
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

			Logger.Info($"Disconnected from server {conn.address}");
		}

		public override void OnStopClient()
		{
			Logger.Info("Stopped client");
		}

		#region Loading Screen

		private void OnPreparingSceneLoad(TCScene scene)
		{
			if (mode == NetworkManagerMode.Offline) return;
			loadingScreenPanel = Instantiate(loadingScreenPrefab).GetComponent<LoadingScreenPanel>();
		}

		private void StartSceneLoad(AsyncOperation sceneLoadOperation)
		{
			if (mode == NetworkManagerMode.Offline || loadingScreenPanel == null) return;

			StartCoroutine(StartSceneLoadAsync(sceneLoadOperation));
		}

		private IEnumerator StartSceneLoadAsync(AsyncOperation sceneLoadOperation)
		{
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
			gameName = message.GameName;

			//Deactivate any deactivated pickups
			//TODO: This stuff should be done in a client pickup manager
			foreach (string unActivePickup in message.DeactivatedPickups)
			{
				GameObject pickup = GameObject.Find(GameManager.GetActiveScene().pickupsParent + unActivePickup);
				if (pickup == null)
				{
					Logger.Error("There was a pickup with the name `{@PickupName}` sent by the server that doesn't exist! Either the server's game is out of date or ours is!", pickup.name);
					continue;
				}

				Pickup pickupLogic = pickup.GetComponent<Pickup>();

				foreach (PickupMaterials pickupMaterials in pickupLogic.pickupMaterials)
				{
					pickupMaterials.meshToChange.material = pickupMaterials.pickupPickedUpMaterial;
				}
			}
		}

		#endregion

		private void SetupNeededSceneStuffClient()
		{
			//Create our the game manager
			Instantiate(gameMangerPrefab);
			Logger.Debug("Created game manager object");
		}

		private IEnumerator UpdateLatency()
		{
			//Update each player's latency from the server's perceptive
			while (mode == NetworkManagerMode.ServerOnly)
			{
				foreach (PlayerManager player in GameManager.GetAllPlayers())
				{
					player.latency = Transport.activeTransport.GetConnectionRtt(player.connectionToClient.connectionId);
				}

				yield return new WaitForSeconds(playerLatencyUpdateTime);
			}
		}

		#region Console Commands

		[ConCommand("connect", "Connects to a server", 1, 1)]
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

		[ConCommand("startserver", "Starts a server", 1, 1)]
		public static void StartServer(string[] args)
		{
			string scene = args[0];
			singleton.onlineScene = scene;

			singleton.StartServer();
		}

		[ConCommand("gamename", "Sets the game name", 1, 100)]
		public static void GameName(string[] args)
		{
			Instance.gameName = string.Join(" ", args);
			Logger.Info("Game name was set to {@Name}", Instance.gameName);
		}
		
		#endregion
	}
}