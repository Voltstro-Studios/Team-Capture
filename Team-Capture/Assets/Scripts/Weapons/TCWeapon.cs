using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		public string weapon;
		public string weaponName;

		public int damage;
		public int range;

		public GameObject baseWeaponPrefab;
	}
}
