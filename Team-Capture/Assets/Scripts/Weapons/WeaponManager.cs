using Mirror;
using Player;
using UnityEngine;

namespace Weapons
{
	public class WeaponManager : NetworkBehaviour
	{
		private class SyncListWeapons : SyncList<string> { }

		[SerializeField] private Transform weaponsHolderSpot;

		private readonly SyncListWeapons weapons = new SyncListWeapons();

		private void Start()
		{
			weapons.Callback += AddWeaponCallback;

			//Create all existing weapons on start
			foreach (string weapon in weapons)
			{
				Instantiate(TCWeaponsManager.GetWeapon(weapon).baseWeaponPrefab, weaponsHolderSpot);
			}
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			//Add stock weapons on client start
			foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons)
			{
				AddWeapon(stockWeapon.weapon);
			}
		}

		private void AddWeaponCallback(SyncList<string>.Operation op, int itemIndex, string item)
		{
			if (op == SyncList<string>.Operation.OP_ADD)
			{
				if (item == null)
				{
					Debug.Log("Item is null");
					return;
				}

				CmdInstantiateWeaponOnClients(item);
			}
		}

		[Command]
		private void CmdInstantiateWeaponOnClients(string weapon)
		{
			RpcInstantiateWeaponOnClients(weapon);
		}

		[ClientRpc]
		private void RpcInstantiateWeaponOnClients(string weapon)
		{
			if (weapon == null)
			{
				return;
			}

			Instantiate(TCWeaponsManager.GetWeapon(weapon).baseWeaponPrefab, weaponsHolderSpot);
		}

		public void AddWeapon(string weapon)
		{
			CmdAddWeapon(transform.name, weapon);
		}

		[Command]
		private void CmdAddWeapon(string playerId, string weapon)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if(player == null)
				return;

			TCWeapon tcWeapon = TCWeaponsManager.GetWeapon(weapon);

			if(tcWeapon == null)
				return;

			player.GetComponent<WeaponManager>().weapons.Add(tcWeapon.weapon);
		}
	}
}
