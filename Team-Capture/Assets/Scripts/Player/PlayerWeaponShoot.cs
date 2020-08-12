using System;
using System.Collections.Generic;
using Core;
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
		/// Layers for the raycast
		/// </summary>
		[SerializeField] private LayerMask raycastLayerMask;

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
			NetworkedWeapon networkedWeapon = weaponManager.GetActiveWeapon();

			//Looks like the weapon isn't setup yet
			if (networkedWeapon == null)
				return;

			TCWeapon weapon = networkedWeapon.GetTCWeapon();

			if(weapon == null)
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

		/// <summary>
		/// Asks the server to shoot this client's weapon
		/// </summary>
		[Command(channel = 3)]
		private void CmdShootWeapon()
		{
			//First, get our active weapon
			NetworkedWeapon activeWeapon = weaponManager.GetActiveWeapon();

			//Reset our nextTimeToFire if we aren't using the same weapon from last time
			if (lastWeapon != activeWeapon.Weapon)
			{
				nextTimeToFire = 0;
				lastWeapon = activeWeapon.Weapon;
			}

			//If our player is dead, or reloading, then return
			if (activeWeapon.IsReloading || playerManager.IsDead || Time.time < nextTimeToFire)
				return;

			if (activeWeapon.CurrentBulletAmount <= 0)
			{
				//Reload
				StartCoroutine(weaponManager.ServerReloadPlayerWeapon());
				return;
			}

			ServerShootWeapon(activeWeapon);

			//Update weapon stats
			weaponManager.TargetSendWeaponStatus(netIdentity.connectionToClient, activeWeapon);
		}

		/// <summary>
		/// Bang bang
		/// </summary>
		/// <param name="networkedWeapon"></param>
		[Server]
		private void ServerShootWeapon(NetworkedWeapon networkedWeapon)
		{
			nextTimeToFire = Time.time + 1f / networkedWeapon.GetTCWeapon().fireRate;

			networkedWeapon.CurrentBulletAmount--;

			RpcWeaponMuzzleFlash();

			SimulationHelper.SimulateCommand(playerManager, () => WeaponRayCast(transform.name));
		}

		/// <summary>
		/// Does a ray cast based of the weapon properties
		/// </summary>
		/// <param name="sourcePlayer"></param>
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

			List<Vector3> targets = new List<Vector3>();
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
					tcWeapon.range, raycastLayerMask);
				foreach (RaycastHit hit in hits)
				{
					//If a player was hit then skip through
					if (playerHit)
						continue;

					//If the hit was the sourcePlayer, then ignore it
					if (hit.collider.name == sourcePlayer)
						continue;

					//Do impact effect on all clients
					RpcWeaponImpact(hit.point, hit.normal, tcWeapon.weapon);
					targets.Add(hit.point);

					//So if we hit a player then do damage
					if (hit.collider.GetComponent<PlayerManager>() == null) continue;
					hit.collider.GetComponent<PlayerManager>().TakeDamage(tcWeapon.damage, sourcePlayer);
					playerHit = true;
				}
			}

			RpcWeaponTracerEffect(targets.ToArray());
		}

		#endregion

		#region Weapon Effects

		/// <summary>
		/// Makes the muzzle flash play
		/// </summary>
		[ClientRpc(channel = 4)]
		private void RpcWeaponMuzzleFlash()
		{
			weaponManager.GetActiveWeaponGraphics().muzzleFlash.Play(true);
		}

		/// <summary>
		/// Makes a bullet hole spawn
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="normal"></param>
		/// <param name="weapon"></param>
		[ClientRpc(channel = 4)]
		private void RpcWeaponImpact(Vector3 pos, Vector3 normal, string weapon)
		{
			TCWeapon tcWeapon = WeaponsResourceManager.GetWeapon(weapon);
			if (tcWeapon == null) return;

			//Instantiate our bullet effects
			Instantiate(tcWeapon.bulletHitEffectPrefab, pos, Quaternion.LookRotation(normal));
			Instantiate(tcWeapon.bulletHolePrefab, pos, Quaternion.FromToRotation(Vector3.back, normal));
		}

		/// <summary>
		/// Make a tracer effect go to the target
		/// </summary>
		/// <param name="targets"></param>
		[ClientRpc(channel = 4)]
		private void RpcWeaponTracerEffect(Vector3[] targets)
		{
			TCWeapon weapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			WeaponGraphics weaponGraphics = weaponManager.GetActiveWeaponGraphics();
			foreach (Vector3 target in targets)
			{
				BulletTracer tracer = Instantiate(weapon.bulletTracerEffect,weaponGraphics.bulletTracerPosition.position, weaponGraphics.bulletTracerPosition.rotation).GetComponent<BulletTracer>();
				tracer.Play(target);
			}
		}

		#endregion
	}
}