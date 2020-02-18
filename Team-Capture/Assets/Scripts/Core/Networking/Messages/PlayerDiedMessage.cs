using Mirror;
using UnityEngine;

namespace Core.Networking.Messages
{
	public class PlayerDiedMessage : MessageBase
	{
		public string PlayerKilled;
		public string PlayerKiller;
		public string WeaponName;
	}
}
