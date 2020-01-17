using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global;
using Helper;
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

		[SerializeField] private string weaponLayerName = "LocalWeapon";

		private void Start()
		{
			weapons.Callback += AddWeaponCallback;

			//Create all existing weapons on start
			for (int i = 0; i < weapons.Count; i++)
			{
				GameObject newWeapon =
					Instantiate(WeaponsResourceManager.GetWeapon(weapons[i]).baseWeaponPrefab, weaponsHolderSpot);

				newWeapon.SetActive(selectedWeaponIndex == i);
			}

			//Add our weapon sway only to the local client
			if (isLocalPlayer)
				weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();
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

				selectedWeaponIndex = itemIndex;

				if (!isLocalPlayer) return;

				CmdInstantiateWeaponOnClients(newItem);

				GetComponent<PlayerManager>().clientUi.hud.UpdateAmmoUi(this);
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

			GameObject newWeapon = Instantiate(WeaponsResourceManager.GetWeapon(weaponName).baseWeaponPrefab, weaponsHolderSpot);
			if (isLocalPlayer)
				Layers.SetLayerRecursively(newWeapon, LayerMask.NameToLayer(weaponLayerName));
		}

		[Command]
		public void CmdSetWeaponIndex(string playerId, int index)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if(player == null) return;

			Logger.Log($"Player {transform.name} set their weapon index to {index}.", LogVerbosity.Debug);

			player.GetComponent<WeaponManager>().selectedWeaponIndex = index;
		}

		#region Weapon Reloading

		public IEnumerator ReloadCurrentWeapon()
		{
			TCWeapon weapon = GetActiveWeapon();

			if (weapon.isReloading)
				yield break;

			Logger.Log($"Reloading weapon `{weapon.weapon}`", LogVerbosity.Debug);

			weapon.currentBulletsAmount = 0;
			weapon.isReloading = true;

			GetComponent<PlayerManager>().clientUi.hud.UpdateAmmoUi(this);

			yield return new WaitForSeconds(weapon.reloadTime);

			weapon.Reload();
			weapon.isReloading = false;

			GetComponent<PlayerManager>().clientUi.hud.UpdateAmmoUi(this);
		}

		#endregion

		public TCWeapon GetActiveWeapon()
		{
			return weapons.Count == 0 ? null : WeaponsResourceManager.GetWeapon(weapons[selectedWeaponIndex]);
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
			if (WeaponsResourceManager.GetWeapon(weaponName) == null) return;

			CmdAddWeapon(transform.name, weaponName);
		}

		[Command]
		private void CmdAddWeapon(string playerId, string weapon)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if (player == null)
				return;

			TCWeapon tcWeapon = WeaponsResourceManager.GetWeapon(weapon);

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
			WeaponsResourceManager.GetWeapon(weapon).Reload();
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

		public void SelectWeapon(int oldValue, int newValue)
		{
			if(!isLocalPlayer)
				return;

			CmdSelectWeapon(transform.name, newValue);

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

			if(isLocalPlayer)
				GetComponent<PlayerManager>().clientUi.hud.UpdateAmmoUi(this);
		}

		#endregion

		public TCWeapon GetWeapon(string weapon)
		{
			IEnumerable<string> result = from a in weapons
				where a == weapon
				select a;

			return WeaponsResourceManager.GetWeapon(result.FirstOrDefault());
		}
	}
}