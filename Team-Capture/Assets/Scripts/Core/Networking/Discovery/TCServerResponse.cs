using System.Net;
using Mirror;

namespace Core.Networking.Discovery
{
	public class TCServerResponse : MessageBase
	{
		public IPEndPoint EndPoint { get; set; }

		public int MaxPlayers;
		public int CurrentAmountOfPlayers;

		public string GameName;

		public string SceneName;
	}
}