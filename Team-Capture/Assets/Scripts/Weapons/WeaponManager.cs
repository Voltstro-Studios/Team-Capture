using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;
using Mirror;
using Player;
using Logger = Global.Logger;

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
			for (int i = 0; i < weapons.Count; i++)
			{
				GameObject newWeapon =
					Instantiate(TCWeaponsManager.GetWeapon(weapons[i]).baseWeaponPrefab, weaponsHolderSpot);

				newWeapon.SetActive(selectedWeaponIndex == i);
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

				if(itemIndex != 0)
					selectedWeaponIndex++;
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
			if (TCWeaponsManager.GetWeapon(weapon) == null) return;

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

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			TargetSetupWeapon(connectionToClient, weapon);
		}

		[TargetRpc]
		private void TargetSetupWeapon(NetworkConnection target, string weapon)
		{
			Global.Logger.Log($"Setup weapon `{weapon}`", LogVerbosity.DEBUG);

			StopCoroutine(ReloadCurrentWeapon());
			TCWeaponsManager.GetWeapon(weapon).Reload();
		}

		#endregion

		[Command]
		public void CmdSetWeaponIndex(int index)
		{
			selectedWeaponIndex = index;
		}

		#region Weapon Selection

		public void SelectWeapon(int index)
		{
			CmdSelectWeapon(transform.name, index);
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

		#endregion

		#region Weapon Reloading

		public IEnumerator ReloadCurrentWeapon()
		{
			Logger.Log($"Reloading weapon `{GetActiveWeapon().weapon}`", LogVerbosity.DEBUG);

			if (GetActiveWeapon().isReloading)
				yield break;

			GetActiveWeapon().isReloading = true;

			yield return new WaitForSeconds(GetActiveWeapon().reloadTime);

			GetActiveWeapon().Reload();

			GetActiveWeapon().isReloading = false;
		}

		#endregion

		public TCWeapon GetActiveWeapon()
		{
			return weapons.Count == 0 ? null : TCWeaponsManager.GetWeapon(weapons[selectedWeaponIndex]);
		}

		public WeaponGraphics GetActiveWeaponGraphics()
		{
			return weaponsHolderSpot.GetChild(selectedWeaponIndex).GetComponent<WeaponGraphics>();
		}
	}
}
