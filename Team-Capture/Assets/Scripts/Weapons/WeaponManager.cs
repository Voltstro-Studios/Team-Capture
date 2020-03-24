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
	public class WeaponManager : NetworkBehaviour
	{
		/// <summary>
		/// A synced list of all the weapons this client has
		/// </summary>
		private readonly SyncListWeapons weapons = new SyncListWeapons();

		/// <summary>
		/// What is the selected weapon
		/// </summary>
		[field: SyncVar(hook = nameof(SelectWeapon))] 
		public int SelectedWeaponIndex { get; private set; }

		/// <summary>
		/// The layer to use when creating our weapons (for local weapons)
		/// </summary>
		[SerializeField] private string weaponLayerName = "LocalWeapon";

		/// <summary>
		/// Where all the weapons are created
		/// </summary>
		[SerializeField] private Transform weaponsHolderSpot;

		/// <summary>
		/// The <see cref="PlayerManager"/> that this <see cref="WeaponManager"/> is associated with
		/// </summary>
		private PlayerManager playerManager;

		/// <summary>
		/// Gets how many weapons are on the weapon holder spot
		/// </summary>
		public int WeaponHolderSpotChildCount => weaponsHolderSpot.childCount;

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
					Instantiate(WeaponsResourceManager.GetWeapon(weapons[i].weapon).baseWeaponPrefab, weaponsHolderSpot);

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

			//Add stock weapons to client
			AddStockWeapons();
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			weapons.Callback += WeaponsOnCallback;

			weaponsHolderSpot.gameObject.AddComponent<WeaponSway>();

			playerManager.clientUi.hud.UpdateAmmoUI(null);
		}

		private void WeaponsOnCallback(SyncList<NetworkedWeapon>.Operation op, int itemindex, NetworkedWeapon olditem, NetworkedWeapon newitem)
		{
			if (op == SyncList<NetworkedWeapon>.Operation.OP_SET)
			{
				Logger.Log("Weapons got updated!");
			}
		}

		#endregion

		[Server]
		private void ServerWeaponCallback(SyncList<NetworkedWeapon>.Operation op, int itemIndex, NetworkedWeapon oldWeapon, NetworkedWeapon newWeapon)
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

		[ClientRpc]
		private void RpcInstantiateWeaponOnClients(string weaponName)
		{
			if (weaponName == null) return;

			GameObject newWeapon = Instantiate(WeaponsResourceManager.GetWeapon(weaponName).baseWeaponPrefab,
				weaponsHolderSpot);
			if (isLocalPlayer)
				Layers.SetLayerRecursively(newWeapon, LayerMask.NameToLayer(weaponLayerName));
		}

		#region Weapon Reloading

		[Client]
		internal void ClientReloadWeapon()
		{
			//Ask the server kindly to reload the weapon
			CmdReloadPlayerWeapon();
		}

		[Command]
		private void CmdReloadPlayerWeapon()
		{
			StartCoroutine(ServerReloadPlayerWeapon());
		}

		[Server]
		public IEnumerator ServerReloadPlayerWeapon()
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

			TCWeapon weapon = networkedWeapon.GetTCWeapon();

			//Update player's UI
			//TargetUpdateAmmoUI(netIdentity.connectionToClient);
			
			yield return new WaitForSeconds(weapon.reloadTime);
			networkedWeapon.currentBulletAmount = weapon.maxBullets;
			networkedWeapon.IsReloading = false;

			//Update player's UI
			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = networkedWeapon.currentBulletAmount,
				IsReloading = false
			});
		}

		#endregion

		internal NetworkedWeapon GetActiveWeapon()
		{
			return weapons.Count == 0 ? null : weapons[SelectedWeaponIndex];
		}

		public WeaponGraphics GetActiveWeaponGraphics()
		{
			return weaponsHolderSpot.GetChild(SelectedWeaponIndex).GetComponent<WeaponGraphics>();
		}

		public TCWeapon GetWeapon(string weapon)
		{
			IEnumerable<NetworkedWeapon> result = from a in weapons
				where a.weapon == weapon
				select a;

			return WeaponsResourceManager.GetWeapon(result.FirstOrDefault()?.weapon);
		}

		private class SyncListWeapons : SyncList<NetworkedWeapon>
		{
		}

		#region Add Weapons

		[Server]
		public void AddStockWeapons()
		{
			foreach (TCWeapon weapon in TCNetworkManager.Instance.stockWeapons)
				AddWeapon(weapon.weapon);
		}

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

			Logger.Log($"Added weapon {weapon} for {transform.name} with {tcWeapon.maxBullets} bullets", LogVerbosity.Debug);

			//Setup the new added weapon, and stop any reloading going on with the current weapon
			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = tcWeapon.maxBullets,
				IsReloading = false
			});

			if (weapons.Count > 1)
			{
				SetClientWeaponIndex(weapons.Count - 1);
			}
		}

		#endregion

		#region Weapon Removal

		[Server]
		public void RemoveAllWeapons()
		{
			SelectedWeaponIndex = 0;
			weapons.Clear();
		}

		[ClientRpc]
		private void RpcRemoveAllActiveWeapons()
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++) Destroy(weaponsHolderSpot.GetChild(i).gameObject);
		}

		#endregion

		#region Weapon Selection

#pragma warning disable IDE0060 // RWe need these variable, Mirror stuff
		public void SelectWeapon(int oldValue, int newValue)
#pragma warning restore IDE0060
		{
			if (!isLocalPlayer)
				return;

			playerManager.clientUi.hud.UpdateAmmoUI(null);
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

		[Server]
		private void SetClientWeaponIndex(int index)
		{
			//Stop reloading
			StopAllCoroutines();

			//Set the selected weapon index and update the visible gameobject
			SelectedWeaponIndex = index;

			//Start reloading weapon if it was reloading before
			if (weapons[index].IsReloading)
				StartCoroutine(ServerReloadPlayerWeapon());

			RpcSelectWeapon(index);
		}

		[ClientRpc]
		private void RpcSelectWeapon(int index)
		{
			for (int i = 0; i < weaponsHolderSpot.childCount; i++)
				weaponsHolderSpot.GetChild(i).gameObject.SetActive(i == index);
		}

		#endregion
	}
}