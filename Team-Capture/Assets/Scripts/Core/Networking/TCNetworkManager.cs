using System.Collections;
using Core.Logger;
using Core.Networking.Discovery;
using LagCompensation;
using Mirror;
using Mirror.LiteNetLib4Mirror;
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
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//Get a spawn point for the player
			Transform spawnPoint = singleton.GetStartPosition();

			//Create the player object
			GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
			player.AddComponent<SimulationObject>();

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
				//Create our own game manager
				Instantiate(gameMangerPrefab);
				Logger.Logger.Log("Created game manager object.", LogVerbosity.Debug);

				//And stop searching for servers
				gameDiscovery.StopDiscovery();
			}
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
	}
}