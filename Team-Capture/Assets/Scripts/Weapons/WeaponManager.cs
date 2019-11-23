using Mirror;
using Player;
using UnityEngine;

namespace Weapons
{
	public class WeaponManager : NetworkBehaviour
	{
		private class SyncListWeapons : SyncList<string> { }

		public Transform weaponsHolderSpot;

		private readonly SyncListWeapons weapons = new SyncListWeapons();

		[SyncVar(hook = nameof(SelectWeapon))] public int selectedWeaponIndex;

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

		#region Add Weapons

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


			WeaponManager weaponManager = player.GetComponent<WeaponManager>();
			weaponManager.weapons.Add(tcWeapon.weapon);
		}

		#endregion

		public void SelectWeapon(int index)
		{
			CmdSelectWeapon(transform.name, index);
		}

		[Command]
		public void CmdSetWeaponIndex(int index)
		{
			selectedWeaponIndex = index;
		}

		[Command]
		public void CmdSelectWeapon(string player, int index)
		{
			if(GameManager.GetPlayer(player) == null)
				return;

			RpcSelectWeapon(player, index);
		}

		[ClientRpc]
		private void RpcSelectWeapon(string player, int index)
		{
			WeaponManager weaponManager = GameManager.GetPlayer(player).GetComponent<WeaponManager>();

			for (int i = 0; i < weaponManager.weaponsHolderSpot.childCount; i++)
			{
				if(i == index)
					weaponManager.weaponsHolderSpot.GetChild(i).gameObject.SetActive(true);
				else
					weaponManager.weaponsHolderSpot.GetChild(i).gameObject.SetActive(false);
			}
		}
	}
}
