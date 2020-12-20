using Team_Capture.Player;
using UnityEngine;

namespace Team_Capture.Pickups
{
	/// <summary>
	///     A health pack, if ya don't know what a health pack is then you are officially retarded
	/// </summary>
	public class HealthPickup : Pickup
	{
		/// <summary>
		///     The amount of health to give
		/// </summary>
		[Tooltip("The amount of health to give")] [SerializeField]
		private int givenHealthAmount = 30;

		protected override void OnPlayerPickup(PlayerManager player)
		{
			//The player is at max health
			if (player.Health == player.MaxHealth)
				return;

			player.AddHealth(givenHealthAmount);

			base.OnPlayerPickup(player);
		}
	}
}