using Player;
using UnityEngine;

public class ClientUI : MonoBehaviour
{
	[HideInInspector] public PlayerManager player;

	public Hud hud;

	public ClientUI SetupUi(PlayerManager playerManager)
	{
		player = playerManager;

		hud.clientUi = this;

		return this;
	}
}
