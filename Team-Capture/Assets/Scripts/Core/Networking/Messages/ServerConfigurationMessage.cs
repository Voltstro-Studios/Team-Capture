using Mirror;

namespace Team_Capture.Core.Networking.Messages
{
	internal struct ServerConfigurationMessage : NetworkMessage
	{
		public ServerConfig ServerConfig;
	}
}