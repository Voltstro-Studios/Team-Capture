using Global;
using Mirror;
using UnityEngine;
using Logger = Global.Logger;

public class TCNetworkManager : NetworkManager
{
	[Header("Team Capture")] [SerializeField]
	private GameObject gameMangerPrefab;

	public override void OnStartServer()
	{
		base.OnStartServer();

		Logger.Log("Server Initialized!");
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		Instantiate(gameMangerPrefab);
		Logger.Log("Created game manager object.", LogVerbosity.Debug);

		base.OnClientSceneChanged(conn);
	}
}