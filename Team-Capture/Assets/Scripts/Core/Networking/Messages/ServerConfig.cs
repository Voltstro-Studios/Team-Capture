using Mirror;

namespace Core.Networking.Messages
{
	[System.Serializable]
	public class ServerConfig : NetworkMessage
	{
		public string gameName = "Team-Capture game";
	}
}