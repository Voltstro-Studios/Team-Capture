using Mirror;

namespace Core.Networking.Messages
{
	[System.Serializable]
	internal class ServerConfig : NetworkMessage
	{
		public string gameName = "Team-Capture game";
	}
}