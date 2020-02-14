using Mirror;
using UnityEngine;

namespace Core.Networking
{
	public class ClientServerInfoMessage : MessageBase
	{
		public GameObject[] UnActivePickups;

		public string GameName;
	}
}