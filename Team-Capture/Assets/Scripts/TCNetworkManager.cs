using Global;
using LagCompensation;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Logger = Global.Logger;

public class TCNetworkManager : LiteNetLib4MirrorNetworkManager
{
	[Header("Team Capture")] 
	[SerializeField] private GameObject gameMangerPrefab;

	[SerializeField] private int maxFrameCount = 128;

	public static int MaxFrameCount;

	public override void Start()
	{
		base.Start();

		MaxFrameCount = maxFrameCount;
	}

	public override void LateUpdate()
	{
		base.LateUpdate();

		if(isNetworkActive)
			SimulationHelper.UpdateSimulationObjectData();
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		base.OnServerReady(conn);

		Logger.Log("Server is ready!");
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

		Instantiate(gameMangerPrefab);
		Logger.Log("Created game manager object.", LogVerbosity.Debug);
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		Transform spawnPoint = NetworkManager.singleton.GetStartPosition();

		GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
		player.AddComponent<SimulationObject>();

		NetworkServer.AddPlayerForConnection(conn, player);
	}

}