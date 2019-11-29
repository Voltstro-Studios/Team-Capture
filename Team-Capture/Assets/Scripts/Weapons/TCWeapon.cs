using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team Capture/TCWeapon")]
    public class TCWeapon : ScriptableObject
    {
        public GameObject baseWeaponPrefab;

        public string weapon;
        public string weaponName;

		public int damage;
		public int range;
		public int fireRate;

		public int maxBullets;

		public float reloadTime = 2.0f;

		[HideInInspector] public int currentBulletsAmount;
		[HideInInspector] public bool isReloading;

		public void Reload()
		{
			currentBulletsAmount = maxBullets;
		}
	}
}

