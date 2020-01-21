using System;
using Global;
using LagCompensation;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Logger = Global.Logger;

public class TCNetworkManager : LiteNetLib4MirrorNetworkManager
{
	public static int MaxFrameCount;

	[Header("Team Capture")] [SerializeField]
	private GameObject gameMangerPrefab;

	[SerializeField] private int maxFrameCount = 128;

	public override void Start()
	{
		base.Start();

		MaxFrameCount = maxFrameCount;
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

		Logger.Log($"Server changed scene to `{sceneName}`.");

		Instantiate(gameMangerPrefab);
		Logger.Log("Created GameManager object.", LogVerbosity.Debug);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

		if (mode != NetworkManagerMode.Host)
		{
			Instantiate(gameMangerPrefab);
			Logger.Log("Created game manager object.", LogVerbosity.Debug);
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		Transform spawnPoint = NetworkManager.singleton.GetStartPosition();

		GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
		player.AddComponent<SimulationObject>();

		NetworkServer.AddPlayerForConnection(conn, player);
	}
}