using Mirror;

namespace Player
{
	public class PlayerManager : NetworkBehaviour
	{
		[SyncVar] public string username;

		public void CmdSetUsername(string playerId, string username)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if (player != null)
			{
				player.username = username;
			}
		}
	}
}
