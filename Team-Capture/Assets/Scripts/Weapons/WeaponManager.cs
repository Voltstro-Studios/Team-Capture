using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Logger;
using Core.Networking;
using Core.Networking.Messages;
using Helper;
using Mirror;
using Player;
using UnityEngine;
using Logger = Core.Logger.Logger;

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
		/// The <see cref="PlayerManager"/> that this <see cref="WeaponManager"/> is associated with
		/// </summary>
		private PlayerManager playerManager;

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
		[field: SyncVar(hook = nameof(SelectWeapon))]
		public int SelectedWeaponIndex { get; private set; }

		/// <summary>
		/// Gets how many weapons are on the weapon holder spot
		/// </summary>
		public int WeaponHolderSpotChildCount => weaponsHolderSpot.childCount;

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
					Logger.Log("Passed in weapon to be added is null!", LogVerbosity.Error);
					weapons.Remove(weapons[itemIndex]);
					return;
				case SyncList<NetworkedWeapon>.Operation.OP_ADD:
					RpcInstantiateWeaponOnClients(newWeapon.weapon);
					break;
				case SyncList<NetworkedWeapon>.Operation.OP_CLEAR:
					RpcRemoveAllActiveWeapons();
					break;
			}
		}

		#region Unity Event Functions

		private void Awake()
		{
			//Assign the player manager, so we can refer to it later
			playerManager = GetComponent<PlayerManager>();
		}

		private void Start()
		{
			//Create all existing weapons on start
			for (int i = 0; i < weapons.Count; i++)
			{
				GameObject newWeapon =
					Instantiate(WeaponsResourceManager.GetWeapon(weapons[i].weapon).baseWeaponPrefab,
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

			weapons.Callback += WeaponsOnCallback;

			weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();

			playerManager.ClientUi.hud.UpdateAmmoUI(null);
		}

		private void WeaponsOnCallback(SyncList<NetworkedWeapon>.Operation op, int itemindex, NetworkedWeapon olditem,
			NetworkedWeapon newitem)
		{
			if (op == SyncList<NetworkedWeapon>.Operation.OP_SET) Logger.Log("Weapons got updated!");
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
				where a.weapon == weapon
				select a;

			return WeaponsResourceManager.GetWeapon(result.FirstOrDefault()?.weapon);
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
		[Command]
		private void CmdReloadPlayerWeapon()
		{
			reloadingCoroutine = StartCoroutine(ServerReloadPlayerWeapon());
		}

		/// <summary>
		/// Reloads clients current weapon
		/// </summary>
		/// <returns></returns>
		[Server]
		internal IEnumerator ServerReloadPlayerWeapon()
		{
			Logger.Log($"Reloading player `{transform.name}`'s active weapon", LogVerbosity.Debug);

			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = 0,
				IsReloading = true
			});

			//Get our players weapon
			NetworkedWeapon networkedWeapon = GetActiveWeapon();
			networkedWeapon.IsReloading = true;

			int weaponIndex = SelectedWeaponIndex;

			TCWeapon weapon = networkedWeapon.GetTCWeapon();

			yield return new WaitForSeconds(weapon.reloadTime);
			FinishReload(networkedWeapon, weapon, weaponIndex);
		}

		[Server]
		private void FinishReload(NetworkedWeapon weapon, TCWeapon tcWeapon, int weaponIndex)
		{
			weapon.currentBulletAmount = tcWeapon.maxBullets;
			weapon.IsReloading = false;

			//Update player's UI
			if(SelectedWeaponIndex != weaponIndex) return;
			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = weapon.currentBulletAmount,
				IsReloading = false
			});
		}

		#endregion

		#region Add Weapons

		/// <summary>
		/// Adds the scene's stock weapons to the client
		/// </summary>
		[Server]
		public void AddStockWeapons()
		{
			foreach (TCWeapon weapon in TCNetworkManager.Instance.stockWeapons)
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

			weapons.Add(new NetworkedWeapon
			{
				weapon = tcWeapon.weapon,
				currentBulletAmount = tcWeapon.maxBullets
			});

			Logger.Log($"Added weapon {weapon} for {transform.name} with {tcWeapon.maxBullets} bullets",
				LogVerbosity.Debug);

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = tcWeapon.maxBullets,
				IsReloading = false
			});

			if (weapons.Count > 1) SetClientWeaponIndex(weapons.Count - 1);
		}

		/// <summary>
		/// Instantiates a weapon model in all clients
		/// </summary>
		/// <param name="weaponName"></param>
		[ClientRpc]
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
		[ClientRpc]
		private void RpcRemoveAllActiveWeapons()
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++) Destroy(weaponsHolderSpot.GetChild(i).gameObject);
		}

		#endregion

		#region Weapon Selection

		/// <summary>
		/// Hook for when the <see cref="SelectedWeaponIndex"/> changes
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
#pragma warning disable IDE0060 //We need these variable, Mirror stuff
		public void SelectWeapon(int oldValue, int newValue)
#pragma warning restore IDE0060
		{
			if (!isLocalPlayer)
				return;

			playerManager.ClientUi.hud.UpdateAmmoUI(null);
		}

		/// <summary>
		/// Sets the <see cref="SelectedWeaponIndex"/> to your index
		/// </summary>
		/// <param name="index"></param>
		[Command]
		public void CmdSetWeapon(int index)
		{
			Logger.Log($"Player `{transform.name}` set their weapon index to `{index}`.", LogVerbosity.Debug);

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
		}

		/// <summary>
		/// Changes the weapons <see cref="GameObject"/> active on this client
		/// </summary>
		/// <param name="index"></param>
		[ClientRpc]
		private void RpcSelectWeapon(int index)
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++)
				weaponsHolderSpot.GetChild(i).gameObject.SetActive(i == index);
		}

		#endregion
	}
}