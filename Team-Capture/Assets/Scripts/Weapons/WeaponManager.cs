using Global;
using Mirror;
using Player;
using UnityEngine;
using Logger = Global.Logger;

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

		[Command]
		public void CmdEquipWeapon(string playerId, string weapon)
		{
			RpcEquipWeapon(playerId, weapon);
		}

		[ClientRpc]
		private void RpcEquipWeapon(string playerId, string weapon)
		{
			TCWeapon tcWeapon = TCWeaponsManager.GetWeapon(weapon);
			PlayerManager player = GameManager.GetPlayer(playerId);

			if (player == null)
			{
				Logger.Log("Imputed player was null!", LogVerbosity.ERROR);

				return;
			}

			if (tcWeapon != null)
			{
				GameObject newWeapon = Instantiate(tcWeapon.baseWeaponPrefab,
					player.GetComponent<WeaponManager>().weaponsHolderSpot);

				player.GetComponent<WeaponManager>().activeWeapons.Add(newWeapon);

				Logger.Log($"Added weapon {tcWeapon.weapon} to player {playerId}.", LogVerbosity.DEBUG);

				return;
			}

			Logger.Log("Imputed weapon was null!", LogVerbosity.ERROR);
		}
	}
}
