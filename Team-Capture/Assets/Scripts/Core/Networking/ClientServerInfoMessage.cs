using Mirror;
using UnityEngine;

namespace Core.Networking
{
	public class ClientServerInfoMessage : MessageBase
	{
		public string[] UnActivePickups;

		public string GameName;
	}
}