using Mirror;

namespace Core.Networking.Messages
{
	public class InitialClientJoinMessage : MessageBase
	{
		/// <summary>
		/// The name of this game
		/// </summary>
		public string GameName;

		/// <summary>
		/// All the current deactivated pickups
		/// </summary>
		public string[] DeactivatedPickups;
	}
}