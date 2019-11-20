using Player;
using Weapons;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
	[SerializeField] private TCWeapon weapon;

	private void OnTriggerEnter(Collider other)
	{
		PlayerManager player = other.GetComponent<PlayerManager>();
		if (player != null)
		{
			player.GetComponent<WeaponManager>().CmdEquipWeapon(other.transform.name, weapon.weapon);
		}
	}
}
