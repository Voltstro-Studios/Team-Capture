using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		[Header("Base Weapon Settings")]
		public string weapon;
		public string weaponFormattedName;

		public GameObject baseWeaponPrefab;

		[Header("Weapon Stats")]
		public int damage;
		public int fireRate;
		public int range;
		public float reloadTime = 2.0f;

		[Header("Weapon Bullets")]
		public int maxBullets;
		public int currentBulletsAmount;

		[HideInInspector] public bool isReloading;

		public void Reload()
		{
			currentBulletsAmount = maxBullets;
		}
	}
}