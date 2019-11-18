using Mirror;
using UnityEngine;

namespace Weapons
{
	public class WeaponManager : NetworkBehaviour
	{
		public class SyncListWeapons : SyncList<GameObject> { }

		public Transform weaponsHolderSpot;

		[SerializeField] private SyncListWeapons activeWeapons;

		private void Start()
		{
			foreach (TCWeapon weapon in GameManager.Instance.scene.stockWeapons)
			{
				EquipWeapon(weapon.weapon);
			}
		}

		private void EquipWeapon(string weapon)
		{
			GameObject newWeapon = Instantiate(TCWeaponsManager.GetWeapon(weapon).baseWeaponPrefab, weaponsHolderSpot);
			GetComponent<WeaponManager>().activeWeapons.Add(newWeapon);
		}
	}
}
