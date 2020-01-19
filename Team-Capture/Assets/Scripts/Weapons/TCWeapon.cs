using UnityEngine;

namespace Weapons
{
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		public GameObject baseWeaponPrefab;
		public int currentBulletsAmount;

		[Header("Weapon Stats")] public int damage;

		public int fireRate;

		[HideInInspector] public bool isReloading;

		[Header("Weapon Bullets")] public int maxBullets;

		public int range;
		public float reloadTime = 2.0f;

		[Header("Base Weapon Settings")] public string weapon;

		public string weaponFormattedName;

		public void Reload()
		{
			currentBulletsAmount = maxBullets;
		}
	}
}