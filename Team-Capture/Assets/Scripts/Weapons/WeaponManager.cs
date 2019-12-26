using System.Collections;
using Global;
using Mirror;
using Player;
using UnityEngine;
using Logger = Global.Logger;

namespace Weapons
{
	public class WeaponManager : NetworkBehaviour
	{
		private readonly SyncListWeapons weapons = new SyncListWeapons();

		[SyncVar(hook = nameof(SelectWeapon))] public int selectedWeaponIndex;

		public Transform weaponsHolderSpot;

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
			foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons) AddWeapon(stockWeapon.weapon);
		}

		private void AddWeaponCallback(SyncList<string>.Operation op, int itemIndex, string item, string newItem)
		{
			if (op == SyncList<string>.Operation.OP_ADD)
			{
				if (newItem == null)
				{
					Debug.Log("Item is null");
					return;
				}

				if (!isLocalPlayer) return;

				CmdInstantiateWeaponOnClients(newItem);

				if (itemIndex != 0)
					selectedWeaponIndex++;
			}

			if (op == SyncList<string>.Operation.OP_CLEAR)
			{
				if (!isLocalPlayer) return;

				CmdRemoveAllActiveWeapons();
			}
		}

		[Command]
		private void CmdInstantiateWeaponOnClients(string weapon)
		{
			RpcInstantiateWeaponOnClients(weapon);
		}

		[ClientRpc]
		private void RpcInstantiateWeaponOnClients(string weaponName)
		{
			if (weaponName == null) return;

			Instantiate(TCWeaponsManager.GetWeapon(weaponName).baseWeaponPrefab, weaponsHolderSpot);
		}

		[Command]
		public void CmdSetWeaponIndex(int index)
		{
			selectedWeaponIndex = index;
		}

		#region Weapon Reloading

		public IEnumerator ReloadCurrentWeapon()
		{
			TCWeapon weapon = GetActiveWeapon();

			if (weapon.isReloading)
				yield break;

			Logger.Log($"Reloading weapon `{weapon.weapon}`", LogVerbosity.Debug);

			weapon.isReloading = true;

			yield return new WaitForSeconds(weapon.reloadTime);

			weapon.Reload();

			weapon.isReloading = false;
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

		private class SyncListWeapons : SyncList<string>
		{
		}

		#region Add Weapons

		public void AddWeapon(string weaponName)
		{
			if (TCWeaponsManager.GetWeapon(weaponName) == null) return;

			CmdAddWeapon(transform.name, weaponName);
		}

		[Command]
		private void CmdAddWeapon(string playerId, string weapon)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if (player == null)
				return;

			TCWeapon tcWeapon = TCWeaponsManager.GetWeapon(weapon);

			if (tcWeapon == null)
				return;

			WeaponManager weaponManager = player.GetComponent<WeaponManager>();
			weaponManager.weapons.Add(tcWeapon.weapon);

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			TargetSetupWeapon(weapon);
		}

		[TargetRpc]
		private void TargetSetupWeapon(string weapon)
		{
			Logger.Log($"Setup weapon `{weapon}`", LogVerbosity.Debug);

			StopCoroutine(ReloadCurrentWeapon());
			TCWeaponsManager.GetWeapon(weapon).Reload();
		}

		#endregion

		#region Weapon Removal

		[Command]
		public void CmdRemoveAllWeapons()
		{
			weapons.Clear();
		}

		[Command]
		private void CmdRemoveAllActiveWeapons()
		{
			RpcRemoveAllActiveWeapons();
		}

		[ClientRpc]
		private void RpcRemoveAllActiveWeapons()
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++) Destroy(weaponsHolderSpot.GetChild(i).gameObject);
		}

		#endregion

		#region Weapon Selection

		public void SelectWeapon(int index)
		{
			CmdSelectWeapon(transform.name, index);
		}

		[Command]
		public void CmdSelectWeapon(string player, int index)
		{
			if (GameManager.GetPlayer(player) == null)
				return;

			RpcSelectWeapon(player, index);
		}

		[ClientRpc]
		private void RpcSelectWeapon(string player, int index)
		{
			WeaponManager weaponManager = GameManager.GetPlayer(player).GetComponent<WeaponManager>();

			for (int i = 0; i < weaponManager.weaponsHolderSpot.childCount; i++)
				if (i == index)
					weaponManager.weaponsHolderSpot.GetChild(i).gameObject.SetActive(true);
				else
					weaponManager.weaponsHolderSpot.GetChild(i).gameObject.SetActive(false);
		}

		#endregion
	}
}