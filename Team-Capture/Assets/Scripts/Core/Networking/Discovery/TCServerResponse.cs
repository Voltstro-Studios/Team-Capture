using System.Net;
using Mirror;

namespace Core.Networking.Discovery
{
	public class TCServerResponse : MessageBase
	{
		public IPEndPoint EndPoint { get; set; }

		public int MaxPlayers { get; set; }

		public string GameName { get; set; }

		public string SceneName { get; set; }
	}
}