using System;
using System.Collections;
using Attributes;
using Core.Networking.Discovery;
using Core.Networking.Messages;
using ENet;
using LagCompensation;
using Mirror;
using Pickups;
using Player;
using SceneManagement;
using UI.Panels;
using UnityEngine;
using Weapons;

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
			base.Start();

			TCScenesManager.PreparingSceneLoadEvent += OnPreparingSceneLoad;
			TCScenesManager.StartSceneLoadEvent += StartSceneLoad;

			//LiteNetLib4MirrorTransport.Singleton.maxConnections = (ushort)maxConnections;
		}

		public void FixedUpdate()
		{
			//If we are playing, then update our simulation objects
			if (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
			{
				SimulationHelper.UpdateSimulationObjectData();

				/*foreach (NetPeer peer in LiteNetLib4MirrorServer.Peers)
				{
					if (peer != null)
					{
						Logger.Logger.Log(peer.Id.ToString());
					}
				}
				*/
			}
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);

			ServerPickupManager.ClearUnActivePickupsList();

			Logging.Logger.Info("Server changed scene to `{@SceneName}`.", sceneName);

			//Instantiate the new game manager
			Instantiate(gameMangerPrefab);
			Logging.Logger.Debug("Created GameManager object.");

			//Setup pickups
			GameObject[] pickups = GameObject.FindGameObjectsWithTag(pickupTagName);
			foreach (GameObject pickup in pickups)
			{
				Pickup pickupLogic = pickup.GetComponent<Pickup>();
				if(pickupLogic == null)
				{
					Logging.Logger.Error("The pickup with the name of `{@PickupName}` @ {@PickupTransform} doesn't have the {@Pickup} behaviour on it!", pickup.name, pickup.transform, typeof(Pickup));
					continue;
				}

				//Setup the trigger
				pickupLogic.SetupTrigger();
			}
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			conn.Send(new InitialClientJoinMessage
			{
				GameName = gameName,
				DeactivatedPickups = ServerPickupManager.GetUnActivePickups()
			});

			//Create the player object
			GameObject player = Instantiate(playerPrefab);
			player.AddComponent<SimulationObject>();

			//We need to add a rigid body, but we don't want to do physics, so also set kinematic to true
			player.AddComponent<Rigidbody>().isKinematic = true;

			//Add the connection for the player
			NetworkServer.AddPlayerForConnection(conn, player);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			//Start advertising the server when the server starts
			gameDiscovery.AdvertiseServer();

			StartCoroutine(UpdateLatency());

			Logging.Logger.Info("Started server!");
		}

		public override void OnStopServer()
		{
			StopCoroutine(UpdateLatency());

			base.OnStopServer();

			//Stop advertising the server when the server stops
			gameDiscovery.StopDiscovery();
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			NetworkClient.RegisterHandler<InitialClientJoinMessage>(OnServerJoinMessage);

			base.OnClientConnect(conn);

			Logging.Logger.Info("Connected to server `{@Address}` with the net ID of {@ConnectionId}.", conn.address, conn.connectionId);

			if (mode != NetworkManagerMode.Host)
			{
				//Stop searching for servers
				gameDiscovery.StopDiscovery();

				//We need to call it here as well, since OnClientChangeScene isn't called when first connecting to a server
				SetupNeededSceneStuffClient();
			}
		}

		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
		{
			base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

			if (mode != NetworkManagerMode.ClientOnly) return;

			Logging.Logger.Info($"The server has changed the scene to `{newSceneName}`.");

			SetupNeededSceneStuffClient();
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

			Logging.Logger.Info($"Disconnected from server `{conn.address}`.");
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

			gameName = message.GameName;

			foreach (string unActivePickup in message.DeactivatedPickups)
			{
				GameObject pickup = GameObject.Find(GameManager.GetActiveScene().pickupsParent + unActivePickup);
				if (pickup == null)
				{
					Logging.Logger.Error("There was a pickup with the name `{@PickupName}` sent by the server that doesn't exist! Either the server's game is out of date or ours is!", pickup.name);
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
			//Don't want to do this stuff in host mode, since we are also the server
			if (mode == NetworkManagerMode.Host) return;

			//Create our own game manager
			Instantiate(gameMangerPrefab);
			Logging.Logger.Debug("Created game manager object.");
		}

		private IEnumerator UpdateLatency()
		{
			while (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
			{
				foreach (PlayerManager player in GameManager.GetAllPlayers())
				{
					player.latency = GetPlayerRtt(player.netId);
				}

				yield return new WaitForSeconds(3.0f);
			}
		}

		public static uint GetPlayerRtt(uint netId)
		{
			if (IgnoranceThreaded.ConnectionIDToPeers.TryGetValue((int)netId - 1, out Peer peer))
			{
				return peer.RoundTripTime;
			}

			if (netId == 1 && GameManager.GetPlayer($"Player {netId}").IsHostPlayer)
			{
				//Shouldn't be any lag for a host player
				return 0;
			}
			
			throw new ArgumentException($"No connection with ID {netId}!");
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
				Logging.Logger.Error("An error occured: {@Error}", e);
			}
		}

		[ConCommand("disconnect", "Disconnect from the current game")]
		public static void DisconnectCommand(string[] args)
		{
			if (singleton.mode == NetworkManagerMode.Offline)
			{
				Logging.Logger.Error("You are not in a game!");
				return;
			}

			singleton.StopHost();
		}
		
		#endregion
	}
}