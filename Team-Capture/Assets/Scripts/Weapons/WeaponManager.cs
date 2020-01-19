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

		private PlayerManager playerManager;

		private void Start()
		{
			playerManager = GetComponent<PlayerManager>();

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

			Logger.Log("Weapon manager is now ready!");

			//Add stock weapons on client start
			CmdAddStockWeapons();

			playerManager.clientUi.hud.UpdateAmmoUi(this);
		}

		private void AddWeaponCallback(SyncList<string>.Operation op, int itemIndex, string item, string newItem)
		{
			//TODO: Make all of this happen only on the server
			if (op == SyncList<string>.Operation.OP_ADD)
			{
				if (newItem == null)
				{
					Debug.Log("Item is null");
					return;
				}

				if (!isLocalPlayer) return;

				CmdInstantiateWeaponOnClients(newItem);
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
		public void CmdSetWeaponIndex(int index)
		{
			Logger.Log($"Player `{transform.name}` set their weapon index to `{index}`.", LogVerbosity.Debug);

			selectedWeaponIndex = index;
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

		[Command]
		public void CmdAddStockWeapons()
		{
			foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons)
				AddWeapon(stockWeapon.weapon);
		}

		/// <summary>
		/// A command, that calls the server side function <see cref="ServerAddWeapon"/>.
		/// This function adds a weapon to a player
		/// </summary>
		/// <param name="weaponName"></param>
		[Command]
		public void CmdAddWeapon(string weaponName)
		{
			ServerAddWeapon(weaponName);
		}

		/// <summary>
		/// Direct server command.
		/// This function adds a weapon to a player
		/// </summary>
		/// <param name="weaponName"></param>
		[Server]
		public void ServerAddWeapon(string weaponName)
		{
			if (WeaponsResourceManager.GetWeapon(weaponName) == null) return;

			AddWeapon(weaponName);
		}

		[Server]
		private void AddWeapon(string weapon)
		{
			TCWeapon tcWeapon = WeaponsResourceManager.GetWeapon(weapon);

			if (tcWeapon == null)
				return;

			weapons.Add(tcWeapon.weapon);

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			TargetSetupWeapon(weapon);

			if(weapons.Count > 1)
				selectedWeaponIndex += 1;
		}

		[TargetRpc]
		private void TargetSetupWeapon(string weapon)
		{
			Logger.Log($"Setting up weapon `{weapon}`", LogVerbosity.Debug);

			StopCoroutine(ReloadCurrentWeapon());
			WeaponsResourceManager.GetWeapon(weapon).Reload();
		}

		#endregion

		#region Weapon Removal

		[Server]
		public void CmdRemoveAllWeapons()
		{
			selectedWeaponIndex = 0;
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

			CmdSelectWeapon(newValue);
		}

		[Command]
		public void CmdSelectWeapon(int index)
		{
			RpcSelectWeapon(index);
		}

		[ClientRpc]
		private void RpcSelectWeapon(int index)
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++)
				if (i == index)
					weaponsHolderSpot.GetChild(i).gameObject.SetActive(true);
				else
					weaponsHolderSpot.GetChild(i).gameObject.SetActive(false);

			if(isLocalPlayer)
				playerManager.clientUi.hud.UpdateAmmoUi(this);
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