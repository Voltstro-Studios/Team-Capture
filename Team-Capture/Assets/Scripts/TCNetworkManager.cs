using Global;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Logger = Global.Logger;

public class TCNetworkManager : LiteNetLib4MirrorNetworkManager
{
	[Header("Team Capture")] [SerializeField]
	private GameObject gameMangerPrefab;

	public override void OnStartServer()
	{
		base.OnStartServer();

		Logger.Log("Server Initialized!");
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		Logger.Log($"Connected to server `{conn.address}` with the net ID of {conn.connectionId}.");

		Instantiate(gameMangerPrefab);
		Logger.Log("Created game manager object.", LogVerbosity.Debug);
	}
}