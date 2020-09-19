using System;
using System.Collections.Generic;
using Core;
using Helper;
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

		#region Mirror Events

		public override void OnStartServer()
		{
			base.OnStartServer();

			localPlayerCamera = gameObject.GetComponent<PlayerSetup>().GetPlayerCamera();
		}

		#endregion

		#region Server Variables

		/// <summary>
		/// (Server only) The local player's camera
		/// </summary>
		private Camera localPlayerCamera;

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

			SimulationHelper.SimulateCommand(playerManager, () => WeaponRayCast());
		}

		/// <summary>
		/// Does a ray cast based of the weapon properties
		/// </summary>
		[Server]
		private void WeaponRayCast()
		{
			//Next, get what weapon the player was using
			TCWeapon tcWeapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			if (tcWeapon == null)
				return;

			//Get the direction the player was facing
			Transform playerFacingDirection = localPlayerCamera.transform;

			//Create a list here, so we know later where the bullets landed
			List<Vector3> targets = new List<Vector3>();
			List<Vector3> targetsNormal = new List<Vector3>();
			for (int i = 0; i < tcWeapon.bulletsPerShot; i++)
			{
				//Calculate random spread
				Vector3 direction = playerFacingDirection.forward;
				direction += playerFacingDirection.TransformDirection(new Vector3(
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor),
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor),
					Random.Range(-tcWeapon.spreadFactor, tcWeapon.spreadFactor)));

				//Now do our raycast
				// ReSharper disable once Unity.PreferNonAllocApi
				RaycastHit[] hits = RaycastHelper.RaycastAllSorted(playerFacingDirection.position, direction,
					tcWeapon.range, raycastLayerMask);
				foreach (RaycastHit hit in hits)
				{
					//Don't count if we hit the shooting player
					if(hit.collider.name == transform.name)
						continue;

					//Do impact effect on all clients
					targets.Add(hit.point);
					targetsNormal.Add(hit.normal);

					//So if we hit a player then do damage
					PlayerManager hitPlayer = hit.collider.GetComponent<PlayerManager>();
					if (hitPlayer == null) break;

					hitPlayer.TakeDamage(tcWeapon.damage, transform.name);
					break;
				}
			}

			//Send where the bullets hit in one big message
			RpcDoWeaponShootEffects(targets.ToArray(), targetsNormal.ToArray());
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
		/// Make a tracer effect go to the target
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="targetNormals"></param>
		[ClientRpc(channel = 4)]
		private void RpcDoWeaponShootEffects(Vector3[] targets, Vector3[] targetNormals)
		{
			TCWeapon weapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			WeaponGraphics weaponGraphics = weaponManager.GetActiveWeaponGraphics();

			for (int i = 0; i < targets.Length; i++)
			{
				//Do bullet tracer
				BulletTracer tracer = Instantiate(weapon.bulletTracerEffect,weaponGraphics.bulletTracerPosition.position, weaponGraphics.bulletTracerPosition.rotation).GetComponent<BulletTracer>();
				tracer.Play(targets[i]);

				//Do bullet holes
				Instantiate(weapon.bulletHitEffectPrefab, targets[i], Quaternion.LookRotation(targetNormals[i]));
				Instantiate(weapon.bulletHolePrefab, targets[i], Quaternion.FromToRotation(Vector3.back, targetNormals[i]));
			}
		}

		#endregion
	}
}