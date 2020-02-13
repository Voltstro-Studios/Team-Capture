using System.Collections;
using Core.Logger;
using Core.Networking.Discovery;
using LagCompensation;
using Mirror;
using Mirror.LiteNetLib4Mirror;
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
			singleton = this;
		}

		public override void Start()
		{
			base.Start();

			TCScenesManager.PreparingSceneLoadEvent += OnPreparingSceneLoad;
			TCScenesManager.StartSceneLoadEvent += StartSceneLoad;

			LiteNetLib4MirrorTransport.Singleton.maxConnections = (ushort)maxConnections;
		}

		public override void LateUpdate()
		{
			base.LateUpdate();

			//If we are playing, then update our simulation objects
			if (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
				SimulationHelper.UpdateSimulationObjectData();
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);

			Logger.Logger.Log($"Server changed scene to `{sceneName}`.");

			//Instantiate the new game manager
			Instantiate(gameMangerPrefab);
			Logger.Logger.Log("Created GameManager object.", LogVerbosity.Debug);

			//Setup pickups
			GameObject[] pickups = GameObject.FindGameObjectsWithTag(pickupTagName);
			foreach (GameObject pickup in pickups)
			{
				Pickup pickupLogic = pickup.GetComponent<Pickup>();
				if(pickupLogic == null)
				{
					Logger.Logger.Log($"The pickup with the name of `{pickup.name}` @ {pickup.transform} doesn't have the {typeof(Pickup)} behaviour on it!", LogVerbosity.Error);
					continue;
				}

				//Setup the trigger
				pickupLogic.SetupTrigger();
			}
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//Get a spawn point for the player
			Transform spawnPoint = singleton.GetStartPosition();

			//Create the player object
			GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
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

			Logger.Logger.Log("Started server!");
		}

		public override void OnStopServer()
		{
			base.OnStopServer();

			//Stop advertising the server when the server stops
			gameDiscovery.StopDiscovery();
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);

			Logger.Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

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

			Logger.Logger.Log($"The server has changed the scene to `{newSceneName}`.");

			SetupNeededSceneStuffClient();
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

			Logger.Logger.Log($"Disconnected from server `{conn.address}`.");
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

		private void SetupNeededSceneStuffClient()
		{
			//Don't want to do this stuff in host mode, since we are also the server
			if (mode == NetworkManagerMode.Host) return;

			//Create our own game manager
			Instantiate(gameMangerPrefab);
			Logger.Logger.Log("Created game manager object.", LogVerbosity.Debug);

			//Setup pickups
			GameObject[] pickups = GameObject.FindGameObjectsWithTag(pickupTagName);
			foreach (GameObject pickup in pickups)
			{
				Pickup pickupLogic = pickup.GetComponent<Pickup>();
				if(pickupLogic == null) continue;

				//Destroy the logic of pickups since it is handled by the server
				Destroy(pickupLogic);
			}
		}
	}
}