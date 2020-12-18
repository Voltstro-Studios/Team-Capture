using Mirror;

namespace Core.Networking.Messages
{
	internal struct ServerConfigurationMessage : NetworkMessage
	{
		public ServerConfig ServerConfig;
	}
}