using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		public enum WeaponFireMode
		{
			Auto,
			Semi
		}

		[Header("Base Weapon Settings")] public string weapon;
		public string weaponFormattedName;
		public GameObject baseWeaponPrefab;

		[Header("Weapon Stats")] 
		public int damage;
		public float fireRate;
		public WeaponFireMode fireMode;
		public int bulletsPerShot = 1;
		
		public int maxBullets;
		public int range;
		public float reloadTime = 2.0f;

		[Header("Spread")]
		public float spreadFactor = 0.05f;

		[Header("Impact Effects")] 
		public GameObject bulletHolePrefab;
		public GameObject bulletHitEffectPrefab;

		[HideInInspector] public bool isReloading;
	}
}