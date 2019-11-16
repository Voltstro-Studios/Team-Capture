using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Weapons
{
	public class WeaponManager : NetworkBehaviour
	{
		public Transform weaponsHolderSpot;

		[SerializeField] private List<GameObject> activeWeapons;

		private void Start()
		{
			//ResetWeapons();
		}

		public void ResetWeapons()
		{
			CmdResetWeapons(transform.name);

			foreach (TCWeapon weapon in GameManager.Instance.scene.stockWeapons)
			{
				CmdEquipWeapon(transform.name, weapon.weapon);
			}
		}

		[Command]
		public void CmdResetWeapons(string player)
		{
			RpcResetWeapons(player);
		}

		[ClientRpc]
		private void RpcResetWeapons(string player)
		{
			foreach (GameObject weapon in GameManager.GetPlayer(player).GetComponent<WeaponManager>().activeWeapons)
			{
				Destroy(weapon);
				GameManager.GetPlayer(player).GetComponent<WeaponManager>().activeWeapons.Remove(weapon);
			}
		}

		[Command]
		public void CmdEquipWeapon(string player, string weapon)
		{
			RpcEquipWeapon(player, weapon);
		}

		[ClientRpc]
		private void RpcEquipWeapon(string player, string weapon)
		{
			GameObject newWeapon = Instantiate(TCWeaponsManager.GetWeapon(weapon).baseWeaponPrefab, weaponsHolderSpot);
			GameManager.GetPlayer(player).GetComponent<WeaponManager>().activeWeapons.Add(newWeapon);
		}

	}
}
