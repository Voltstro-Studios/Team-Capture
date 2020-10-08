using Mirror;

namespace Core.Networking.Messages
{
	public struct InitialClientJoinMessage : IMessageBase
	{
		//TODO: We should move this to an authenticator
		public ServerConfig ServerConfig;

		public void Deserialize(NetworkReader reader) { }

		public void Serialize(NetworkWriter writer) { }
	}
}