using System;
using Mirror;

namespace Team_Capture.Core.Networking.Messages
{
	[Serializable]
	internal class ServerConfig : NetworkMessage
	{
		public string gameName = "Team-Capture game";
	}
}