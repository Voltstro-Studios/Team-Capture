using Mirror;
using UnityEngine;

namespace Player
{
	public class PlayerManager : NetworkBehaviour
	{
		[SyncVar] public string username = "Not Set";

		[SerializeField] private int maxHealth = 100;
		[SyncVar] private int health;

		public bool IsDead { get; protected set; }
	}
}
