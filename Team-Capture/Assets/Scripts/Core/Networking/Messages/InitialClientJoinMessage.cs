using Mirror;

namespace Core.Networking.Messages
{
	public class InitialClientJoinMessage : MessageBase
	{
		//TODO: We should move this to an authenticator
		public ServerConfig ServerConfig;

		/// <summary>
		/// All the current deactivated pickups
		/// </summary>
		public string[] DeactivatedPickups;
	}
}