using Global;
using UnityEngine;
using Mirror;

public class TCNetworkManager : NetworkManager
{
    [Header("Team Capture")] [SerializeField]
    private GameObject gameMangerPrefab;

    public override void OnStartServer()
	{
		base.OnStartServer();

		Global.Logger.Log("Server Initialized!");
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		Instantiate(gameMangerPrefab);
		Global.Logger.Log("Created game manager object.", LogVerbosity.Debug);

        base.OnClientSceneChanged(conn);
    }
}