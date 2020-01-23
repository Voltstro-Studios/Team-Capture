using System.Collections;
using Core.Logger;
using LagCompensation;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using SceneManagement;
using UI.Panels;
using UnityEngine;
using Weapons;

namespace Core
{
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

		public override void Start()
		{
			base.Start();

			MaxFrameCount = maxFrameCount;
			StockWeapons = stockWeapons;

			TCScenesManager.PreparingSceneLoadEvent += OnPreparingSceneLoad;
			TCScenesManager.StartSceneLoadEvent += StartSceneLoad;
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

		public override void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);

			Logger.Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

			if (mode != NetworkManagerMode.Host)
			{
				Instantiate(gameMangerPrefab);
				Logger.Logger.Log("Created game manager object.", LogVerbosity.Debug);
			}
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();

			GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
			player.AddComponent<SimulationObject>();

			NetworkServer.AddPlayerForConnection(conn, player);
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