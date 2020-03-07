using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		[Header("Base Weapon Settings")] public string weapon;
		public string weaponFormattedName;
		public GameObject baseWeaponPrefab;
		public int currentBulletsAmount;

		[Header("Weapon Stats")] public int damage;
		public int fireRate;
		public int bulletsAmount = 1;
		[HideInInspector] public bool isReloading;

		[Header("Weapon Bullets")] public int maxBullets;
		public int range;
		public float reloadTime = 2.0f;

		[Header("Spread")]
		public float spreadFactor = 0.05f;

		[Header("Impact Effects")] 
		public GameObject bulletHolePrefab;

		public void Reload()
		{
			currentBulletsAmount = maxBullets;
		}
	}
}