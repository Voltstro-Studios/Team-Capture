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

			base.OnPlayerPickup(player);
		}
	}
}
