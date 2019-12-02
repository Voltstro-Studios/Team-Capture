using System;
using UnityEngine;
using Mirror;

namespace Player
{
    public class PlayerManager : NetworkBehaviour
    {
        [SyncVar] public string username = "Not Set";

		[SerializeField] private int maxHealth = 100;
		[SyncVar] private int health;

		public bool IsDead { get; protected set; }

		private void Start()
		{
			health = maxHealth;
		}

		[Command]
		public void CmdTakeDamage(string sourceId, int damage)
		{
			PlayerManager player = GameManager.GetPlayer(sourceId);
			if(player == null)
				return;

			player.health -= damage;

			if (player.health <= 0)
			{
				//Dead
				Debug.Log("Dead");
			}
		}
    }
}
