using Player;
using UnityEngine;

namespace Pickups
{
	public class HealthPickup : Pickup
	{
		[SerializeField] private int givenHealthAmount = 30;

		public override void OnPlayerPickup(PlayerManager player)
		{
			//The player is at max health
			if(player.Health == player.MaxHealth)
				return;

			player.AddHealth(givenHealthAmount);

			//Deactivate the pickup and respawn it
			ServerPickupManager.DeactivatePickup(gameObject);
			StartCoroutine(RespawnPickup());
		}
	}
}
