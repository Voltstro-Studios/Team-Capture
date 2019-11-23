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
			if (player != null)
			{
				player.GetComponent<WeaponManager>().AddWeapon(weapon.weapon);
			}
		}
	}
}
