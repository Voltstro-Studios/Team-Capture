using Mirror;

namespace Player
{
	public class PlayerManager : NetworkBehaviour
	{
		[SyncVar] public string username = "Not Set";

		public void Setup()
		{

		}
	}
}
