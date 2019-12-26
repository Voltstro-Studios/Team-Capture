using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		public GameObject baseWeaponPrefab;

		[HideInInspector] public int currentBulletsAmount;

		public int damage;
		public int fireRate;
		[HideInInspector] public bool isReloading;

		public int maxBullets;
		public int range;

		public float reloadTime = 2.0f;

		public string weapon;
		public string weaponName;

		public void Reload()
		{
			currentBulletsAmount = maxBullets;
		}
	}
}