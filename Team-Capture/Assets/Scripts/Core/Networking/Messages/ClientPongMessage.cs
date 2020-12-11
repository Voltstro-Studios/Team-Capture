using Mirror;

namespace Core.Networking.Messages
{
	public struct ClientPongMessage : NetworkMessage
	{
		public double ClientTime;
	}
}