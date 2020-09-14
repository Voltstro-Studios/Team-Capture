using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Delegates;
using Helper;
using Mirror;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Weapons
{
	/// <summary>
	/// Weapon management, such as adding, removing and selecting weapons
	/// </summary>
	public class WeaponManager : NetworkBehaviour
	{
		/// <summary>
		/// A synced list of all the weapons this client has
		/// </summary>
		private readonly SyncListWeapons weapons = new SyncListWeapons();

		/// <summary>
		/// The active reloading coroutine
		/// </summary>
		private Coroutine reloadingCoroutine;

		/// <summary>
		/// The layer to use when creating our weapons (for local weapons)
		/// </summary>
		[SerializeField] private string weaponLayerName = "LocalWeapon";

		/// <summary>
		/// Where all the weapons are created
		/// </summary>
		[SerializeField] private Transform weaponsHolderSpot;

		/// <summary>
		/// What is the selected weapon
		/// </summary>
		[field: SyncVar]
		public int SelectedWeaponIndex { get; private set; }

		/// <summary>
		/// Gets how many weapons are on the weapon holder spot
		/// </summary>
		public int WeaponHolderSpotChildCount => weaponsHolderSpot.childCount;

		public event WeaponUpdated WeaponUpdated;

		/// <summary>
		/// Server callback for when <see cref="weapons"/> is modified
		/// </summary>
		/// <param name="op"></param>
		/// <param name="itemIndex"></param>
		/// <param name="oldWeapon"></param>
		/// <param name="newWeapon"></param>
		[Server]
		private void ServerWeaponCallback(SyncList<NetworkedWeapon>.Operation op, int itemIndex,
			NetworkedWeapon oldWeapon, NetworkedWeapon newWeapon)
		{
			switch (op)
			{
				case SyncList<NetworkedWeapon>.Operation.OP_ADD when newWeapon == null:
					Logger.Error("Passed in weapon to be added is null!");
					weapons.Remove(weapons[itemIndex]);
					return;
				case SyncList<NetworkedWeapon>.Operation.OP_ADD:
					RpcInstantiateWeaponOnClients(newWeapon.Weapon);
					break;
				case SyncList<NetworkedWeapon>.Operation.OP_CLEAR:
					RpcRemoveAllActiveWeapons();
					break;
			}
		}

		#region Unity Event Functions

		private void Start()
		{
			//Create all existing weapons on start
			for (int i = 0; i < weapons.Count; i++)
			{
				GameObject newWeapon =
					Instantiate(WeaponsResourceManager.GetWeapon(weapons[i].Weapon).baseWeaponPrefab,
						weaponsHolderSpot);

				newWeapon.SetActive(SelectedWeaponIndex == i);
			}
		}

		#endregion

		#region Network Overrides

		public override void OnStartServer()
		{
			base.OnStartServer();

			//Setup our add weapon callback
			weapons.Callback += ServerWeaponCallback;
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();
		}

		#endregion

		#region Weapon List Management

		/// <summary>
		/// Get the active weapon
		/// </summary>
		/// <returns></returns>
		internal NetworkedWeapon GetActiveWeapon()
		{
			return weapons.Count == 0 ? null : weapons[SelectedWeaponIndex];
		}

		/// <summary>
		/// Gets the active weapon's graphics
		/// </summary>
		/// <returns></returns>
		internal WeaponGraphics GetActiveWeaponGraphics()
		{
			return weaponsHolderSpot.GetChild(SelectedWeaponIndex).GetComponent<WeaponGraphics>();
		}

		/// <summary>
		/// Gets a weapon from the list
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		internal TCWeapon GetWeapon(string weapon)
		{
			IEnumerable<NetworkedWeapon> result = from a in weapons
				where a.Weapon == weapon
				select a;

			return WeaponsResourceManager.GetWeapon(result.FirstOrDefault()?.Weapon);
		}

		private class SyncListWeapons : SyncList<NetworkedWeapon>
		{
		}

		#endregion

		#region Weapon Reloading

		/// <summary>
		/// Requests the server to reload the current weapon
		/// </summary>
		[Client]
		internal void ClientReloadWeapon()
		{
			//Ask the server kindly to reload the weapon
			CmdReloadPlayerWeapon();
		}

		/// <summary>
		/// Reloads clients current weapon
		/// </summary>
		[Command(channel = 3)]
		private void CmdReloadPlayerWeapon()
		{
			NetworkedWeapon weapon = GetActiveWeapon();

			if(weapon.IsReloading)
				return;

			if(weapon.CurrentBulletAmount == weapon.GetTCWeapon().maxBullets)
				return;

			reloadingCoroutine = StartCoroutine(ServerReloadPlayerWeapon());
		}

		/// <summary>
		/// Reloads clients current weapon
		/// </summary>
		/// <returns></returns>
		[Server]
		internal IEnumerator ServerReloadPlayerWeapon()
		{
			Logger.Debug($"Reloading player `{transform.name}`'s active weapon");

			//Get our players weapon
			NetworkedWeapon networkedWeapon = GetActiveWeapon();
			networkedWeapon.IsReloading = true;
			networkedWeapon.CurrentBulletAmount = 0;

			TargetSendWeaponStatus(GetClientConnection, networkedWeapon);

			int weaponIndex = SelectedWeaponIndex;

			TCWeapon weapon = networkedWeapon.GetTCWeapon();

			yield return new WaitForSeconds(weapon.reloadTime);
			FinishReload(networkedWeapon, weaponIndex);
		}

		[Server]
		private void FinishReload(NetworkedWeapon weapon, int weaponIndex)
		{
			weapon.Reload();

			//Update player's UI
			if(SelectedWeaponIndex != weaponIndex) return;
			TargetSendWeaponStatus(GetClientConnection, weapon);
		}

		#endregion

		#region Add Weapons

		/// <summary>
		/// Adds the scene's stock weapons to the client
		/// </summary>
		[Server]
		public void AddStockWeapons()
		{
			foreach (TCWeapon weapon in GameManager.Instance.scene.stockWeapons)
				AddWeapon(weapon.weapon);
		}

		/// <summary>
		/// Adds a weapon
		/// </summary>
		/// <param name="weapon">The weapon to add</param>
		[Server]
		internal void AddWeapon(string weapon)
		{
			TCWeapon tcWeapon = WeaponsResourceManager.GetWeapon(weapon);

			if (tcWeapon == null)
				return;

			NetworkedWeapon netWeapon = new NetworkedWeapon(tcWeapon);
			weapons.Add(netWeapon);

			Logger.Debug($"Added weapon {weapon} for {transform.name} with {tcWeapon.maxBullets} bullets");

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			TargetSendWeaponStatus(GetClientConnection, netWeapon);

			if (weapons.Count > 1) SetClientWeaponIndex(weapons.Count - 1);
		}

		/// <summary>
		/// Instantiates a weapon model in all clients
		/// </summary>
		/// <param name="weaponName"></param>
		[ClientRpc(channel = 3)]
		private void RpcInstantiateWeaponOnClients(string weaponName)
		{
			if (weaponName == null) return;

			GameObject newWeapon = Instantiate(WeaponsResourceManager.GetWeapon(weaponName).baseWeaponPrefab,
				weaponsHolderSpot);
			if (isLocalPlayer)
				Layers.SetLayerRecursively(newWeapon, LayerMask.NameToLayer(weaponLayerName));
		}

		#endregion

		#region Weapon Removal

		/// <summary>
		/// Removes all weapons this client has
		/// </summary>
		[Server]
		public void RemoveAllWeapons()
		{
			SelectedWeaponIndex = 0;
			weapons.Clear();
		}

		/// <summary>
		/// Removes all weapons on the client
		/// </summary>
		[ClientRpc(channel = 4)]
		private void RpcRemoveAllActiveWeapons()
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++) Destroy(weaponsHolderSpot.GetChild(i).gameObject);
		}

		#endregion

		#region Weapon Selection

		/// <summary>
		/// Sets the <see cref="SelectedWeaponIndex"/> to your index
		/// </summary>
		/// <param name="index"></param>
		[Command(channel = 3)]
		public void CmdSetWeapon(int index)
		{
			if(weapons.ElementAt(index) == null)
				return;

			Logger.Debug($"Player `{transform.name}` set their weapon index to `{index}`.");

			SetClientWeaponIndex(index);
		}

		/// <summary>
		/// Sets the <see cref="SelectedWeaponIndex"/>
		/// </summary>
		/// <param name="index"></param>
		[Server]
		private void SetClientWeaponIndex(int index)
		{
			//Stop reloading
			if(reloadingCoroutine != null)
				StopCoroutine(reloadingCoroutine);

			//Set the selected weapon index and update the visible gameobject
			SelectedWeaponIndex = index;

			//Start reloading weapon if it was reloading before
			if (weapons[index].IsReloading)
				StartCoroutine(ServerReloadPlayerWeapon());

			RpcSelectWeapon(index);

			TargetSendWeaponStatus(GetClientConnection, weapons[SelectedWeaponIndex]);
		}

		/// <summary>
		/// Changes the weapons <see cref="GameObject"/> active on this client
		/// </summary>
		/// <param name="index"></param>
		[ClientRpc(channel = 3)]
		private void RpcSelectWeapon(int index)
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++)
				weaponsHolderSpot.GetChild(i).gameObject.SetActive(i == index);
		}

		#endregion

		#region Client Stuff

		[TargetRpc(channel = 4)]
		internal void TargetSendWeaponStatus(NetworkConnection conn, NetworkedWeapon weaponStatus)
		{
			WeaponUpdated?.Invoke(weaponStatus);
		}

		#endregion

		/// <summary>
		/// Gets the server's connection to this client
		/// </summary>
		private NetworkConnection GetClientConnection => netIdentity.connectionToClient;
	}
}