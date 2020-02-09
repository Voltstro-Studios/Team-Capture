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

namespace Core
{
	[RequireComponent(typeof(TCGameDiscovery))]
	public class TCNetworkManager : LiteNetLib4MirrorNetworkManager
	{
		public static int MaxFrameCount;

		public static TCWeapon[] StockWeapons;

		[Header("Team Capture")] [SerializeField]
		private GameObject gameMangerPrefab;

		[SerializeField] private int maxFrameCount = 128;

		[SerializeField] private TCWeapon[] stockWeapons;

		[SerializeField] private GameObject loadingScreenPrefab;
		private LoadingScreenPanel loadingScreenPanel;

		private TCGameDiscovery gameDiscovery;

		public override void Start()
		{
			base.Start();

			MaxFrameCount = maxFrameCount;
			StockWeapons = stockWeapons;

			TCScenesManager.PreparingSceneLoadEvent += OnPreparingSceneLoad;
			TCScenesManager.StartSceneLoadEvent += StartSceneLoad;

			gameDiscovery = GetComponent<TCGameDiscovery>();
			gameDiscovery.StartDiscovery();
		}

		public override void LateUpdate()
		{
			base.LateUpdate();

			if (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
				SimulationHelper.UpdateSimulationObjectData();
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);

			Logger.Logger.Log($"Server changed scene to `{sceneName}`.");

			Instantiate(gameMangerPrefab);
			Logger.Logger.Log("Created GameManager object.", LogVerbosity.Debug);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();

			GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
			player.AddComponent<SimulationObject>();

			NetworkServer.AddPlayerForConnection(conn, player);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			gameDiscovery.AdvertiseServer();

			Logger.Logger.Log("Started server!");
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);

			Logger.Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

			if (mode != NetworkManagerMode.Host)
			{
				Instantiate(gameMangerPrefab);
				Logger.Logger.Log("Created game manager object.", LogVerbosity.Debug);

				gameDiscovery.StopDiscovery();
			}
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);

			Logger.Logger.Log($"Disconnected from server `{conn.address}`.");

			gameDiscovery.StartDiscovery();
		}

		public override void OnStopHost()
		{
			base.OnStopHost();

			gameDiscovery.StartDiscovery();
		}

		#region Loading Screen

		private void OnPreparingSceneLoad(TCScene scene)
		{
			if(mode == NetworkManagerMode.Offline) return;
			loadingScreenPanel = Instantiate(loadingScreenPrefab).GetComponent<LoadingScreenPanel>();
		} 

		private void StartSceneLoad(AsyncOperation sceneLoadOperation)
		{
			if(mode == NetworkManagerMode.Offline || loadingScreenPanel == null) return;

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