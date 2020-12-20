using Team_Capture.Player;
using Team_Capture.Weapons;
using UnityEngine;

namespace Team_Capture.Pickups
{
	/// <summary>
	///     A pickup for a weapon
	/// </summary>
	public class WeaponPickup : Pickup
	{
		/// <summary>
		///     The weapon to give
		/// </summary>
		[Tooltip("The weapon to give")] public TCWeapon weapon;

		protected override void OnPlayerPickup(PlayerManager player)
		{
			WeaponManager weaponManager = player.GetComponent<WeaponManager>();

			//Don't want to pickup the same weapon
			if (weaponManager.GetWeapon(weapon.weapon) != null) return;

			weaponManager.AddWeapon(weapon.weapon);

			base.OnPlayerPickup(player);
		}
	}
}