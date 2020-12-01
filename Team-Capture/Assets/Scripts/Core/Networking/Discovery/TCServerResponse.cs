using System.Net;
using Mirror;

namespace Core.Networking.Discovery
{
	internal class TCServerResponse : NetworkMessage
	{
		public int CurrentAmountOfPlayers;

		public string GameName;

		public int MaxPlayers;

		public string SceneName;
		public IPEndPoint EndPoint { get; set; }
	}
}