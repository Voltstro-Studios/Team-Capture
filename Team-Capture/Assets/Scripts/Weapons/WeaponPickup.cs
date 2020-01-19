using Player;
using UnityEngine;

namespace Weapons
{
	public class WeaponPickup : MonoBehaviour
	{
		[SerializeField] private TCWeapon weapon;

		private void OnTriggerEnter(Collider other)
		{
			PlayerManager player = other.GetComponent<PlayerManager>();
			if (player == null) return;

			if (player.GetComponent<WeaponManager>().GetWeapon(weapon.weapon) == null)
			{
				//TODO: Make the spinning weapon transparent and have a delay before others can pick it up
				player.GetComponent<WeaponManager>().CmdAddWeapon(weapon.weapon);
				player.clientUi.hud.UpdateAmmoUi(player.GetComponent<WeaponManager>());
			}
		}
	}
}