using System;
using Core;
using Core.Networking.Messages;
using LagCompensation;
using Mirror;
using UI;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

namespace Player
{
	/// <summary>
	/// Handles shooting
	/// </summary>
	public class PlayerWeaponShoot : NetworkBehaviour
	{
		/// <summary>
		/// The pickup tag
		/// </summary>
		[SerializeField] private string pickupTag = "Pickup";

		/// <summary>
		/// The <see cref="PlayerManager"/> associated with this <see cref="PlayerWeaponShoot"/>
		/// </summary>
		private PlayerManager playerManager;

		/// <summary>
		/// The <see cref="weaponManager"/> associated with this <see cref="PlayerWeaponShoot"/>
		/// </summary>
		private WeaponManager weaponManager;

		[Client]
		private void ShootWeapon()
		{
			CmdShootWeapon();
		}

		#region Server Variables

		/// <summary>
		/// (Server only) The last weapon this client used
		/// </summary>
		private string lastWeapon;

		/// <summary>
		/// (Server only) The next time to fire
		/// </summary>
		private float nextTimeToFire;

		#endregion

		#region Unity Event Functions

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			if (ClientUI.IsPauseMenuOpen)
				return;

			//Cache our current weapon
			TCWeapon weapon = weaponManager.GetActiveWeapon().GetTCWeapon();

			//Looks like the weapon isn't setup yet
			if (weapon == null)
				return;

			if (playerManager.IsDead)
			{
				CancelInvoke(nameof(CmdShootWeapon));
				return;
			}

			if (Input.GetButtonDown("Reload"))
			{
				weaponManager.ClientReloadWeapon();
				return;
			}

			switch (weapon.fireMode)
			{
				case TCWeapon.WeaponFireMode.Semi:
					if (Input.GetButtonDown("Fire1"))
						ShootWeapon();
					break;
				case TCWeapon.WeaponFireMode.Auto:
					if (Input.GetButtonDown("Fire1"))
						InvokeRepeating(nameof(ShootWeapon), 0f, 1f / weapon.fireRate);
					else if (Input.GetButtonUp("Fire1"))
						CancelInvoke(nameof(ShootWeapon));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Server Side Shooting

		[Command(channel = 1)]
		private void CmdShootWeapon()
		{
			//First, get our active weapon
			NetworkedWeapon activeWeapon = weaponManager.GetActiveWeapon();
			TCWeapon tcWeapon = activeWeapon.GetTCWeapon();

			//Reset our nextTimeToFire if we aren't using the same weapon from last time
			if (lastWeapon != activeWeapon.weapon)
			{
				nextTimeToFire = 0;
				lastWeapon = activeWeapon.weapon;
			}

			//If our player is dead, or reloading, then return
			if (activeWeapon.IsReloading || playerManager.IsDead || Time.time < nextTimeToFire)
				return;

			if (activeWeapon.currentBulletAmount <= 0)
			{
				//Reload
				StartCoroutine(weaponManager.ServerReloadPlayerWeapon());
				return;
			}

			ServerShootWeapon(activeWeapon, tcWeapon);

			//Update weapon stats
			netIdentity.connectionToClient.Send(new WeaponSyncMessage
			{
				CurrentBullets = activeWeapon.currentBulletAmount,
				IsReloading = false
			});
		}

		[Server]
		private void ServerShootWeapon(NetworkedWeapon networkedWeapon, TCWeapon tcWeapon)
		{
			nextTimeToFire = Time.time + 1f / tcWeapon.fireRate;

			networkedWeapon.currentBulletAmount--;

			RpcWeaponMuzzleFlash();

			SimulationHelper.SimulateCommand(playerManager, () => WeaponRayCast(transform.name));
		}

		[Server]
		private void WeaponRayCast(string sourcePlayer)
		{
			//First, get our player
			PlayerManager player = GameManager.GetPlayer(sourcePlayer);
			if (player == null) return;

			//Next, get what weapon the player was using
			TCWeapon tcWeapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			if (tcWeapon == null)
				return;

			//Get the direction the player was facing
			Transform playerFacingDirection = player.GetComponent<PlayerSetup>().GetPlayerCamera().transform;

			for (int i = 0; i < tcWeapon.bulletsPerShot; i++)
			{
				//Calculate random spread
				Vector3 direction = playerFacingDirection.forward;
				direction += playerFacingDirection.TransformDirection(new Vector3(
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor),
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor),
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor)));

				//Was a player hit?
				bool playerHit = false;

				//Now do our raycast
				// ReSharper disable once Unity.PreferNonAllocApi
				RaycastHit[] hits = Physics.RaycastAll(playerFacingDirection.position, direction,
					tcWeapon.range);
				foreach (RaycastHit hit in hits)
				{
					//If a player was hit then skip through
					if (playerHit)
						continue;

					//If the hit was the sourcePlayer, then ignore it
					if (hit.collider.name == sourcePlayer)
						continue;

					//We want bullets to go through pickups
					if (hit.collider.CompareTag(pickupTag))
						continue;

					//Do impact effect on all clients
					RpcWeaponImpact(hit.point, hit.normal, tcWeapon.weapon);

					//So if we hit a player then do damage
					if (hit.collider.GetComponent<PlayerManager>() == null) continue;
					hit.collider.GetComponent<PlayerManager>().TakeDamage(tcWeapon.damage, sourcePlayer);
					playerHit = true;
				}
			}
		}

		#endregion

		#region Weapon Effects

		#region Weapon Muzzle

		[ClientRpc(channel = 3)]
		private void RpcWeaponMuzzleFlash()
		{
			weaponManager.GetActiveWeaponGraphics().muzzleFlash.Play(true);
		}

		#endregion

		#region Weapon Impact

		[ClientRpc(channel = 3)]
		private void RpcWeaponImpact(Vector3 pos, Vector3 normal, string weapon)
		{
			TCWeapon tcWeapon = WeaponsResourceManager.GetWeapon(weapon);
			if (tcWeapon == null) return;

			//Instantiate our bullet effects
			Instantiate(tcWeapon.bulletHitEffectPrefab, pos, Quaternion.LookRotation(normal));
			Instantiate(tcWeapon.bulletHolePrefab, pos, Quaternion.FromToRotation(Vector3.back, normal));
		}

		#endregion

		#endregion
	}
}