using System;
using Mirror;

namespace Core.Networking.Messages
{
	[Serializable]
	internal class ServerConfig : NetworkMessage
	{
		public string gameName = "Team-Capture game";
	}
}