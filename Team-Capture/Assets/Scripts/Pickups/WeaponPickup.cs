using Player;
using Weapons;

namespace Pickups
{
	public class WeaponPickup : Pickup
	{
		public TCWeapon weapon;

		public override void OnPlayerPickup(PlayerManager player)
		{
			WeaponManager weaponManager = player.GetComponent<WeaponManager>();

			//Don't want to pickup the same weapon
			if(weaponManager.GetWeapon(weapon.weapon) != null) return;

			weaponManager.ServerAddWeapon(weapon.weapon);

			//Deactivate the pickup and respawn it
			ServerPickupManager.DeactivatePickup(gameObject);
			StartCoroutine(RespawnPickup());
		}
	}
}