using Mirror;

namespace Team_Capture.Core.Networking.Messages
{
	public struct ClientPongMessage : NetworkMessage
	{
		public double ClientTime;
	}
}