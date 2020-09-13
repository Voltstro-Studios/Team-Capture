using Mirror;

namespace Core.Networking.Messages
{
	[System.Serializable]
	public class ServerConfig : MessageBase
	{
		public string gameName = "Team-Capture game";
	}
}