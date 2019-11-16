using Mirror;
using UnityEngine;

public class TCNetworkManager : NetworkManager
{
	[Header("Team Capture")]
	[SerializeField] private GameObject gameMangerPrefab = null;

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		Instantiate(gameMangerPrefab);

		base.OnClientSceneChanged(conn);
	}
}
