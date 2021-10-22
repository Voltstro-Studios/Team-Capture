// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Diagnostics;
using Mirror;
using Team_Capture.Helper;
using Team_Capture.LagCompensation;
using Team_Capture.Pooling;
using Team_Capture.SceneManagement;
using Team_Capture.Weapons;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Random = UnityEngine.Random;

namespace Team_Capture.Player
{
	/// <summary>
	///     Handles shooting
	/// </summary>
	public sealed class PlayerWeaponShoot : NetworkBehaviour
	{
		/// <summary>
		///     Layers for the raycast
		/// </summary>
		[SerializeField] private LayerMask raycastLayerMask;

		/// <summary>
		///     The <see cref="PlayerManager" /> associated with this <see cref="PlayerWeaponShoot" />
		/// </summary>
		private PlayerManager playerManager;

		/// <summary>
		///     The <see cref="weaponManager" /> associated with this <see cref="PlayerWeaponShoot" />
		/// </summary>
		private WeaponManager weaponManager;

		private GameObjectPool tracerPool;
		private GameObjectPool bulletHolesPool;

		#region Server Variables

		/// <summary>
		///     (Server only) The local player's camera
		/// </summary>
		[SerializeField] private Transform localPlayerCamera;

		/// <summary>
		///     (Server only) The last weapon this client used
		/// </summary>
		private string lastWeapon;

		/// <summary>
		///     (Server only) The next time to fire
		/// </summary>
		private float nextTimeToFire;

		#endregion

		#region Unity Event Functions

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
			playerManager.PlayerDeath += PlayerDeath;

			tracerPool = GameSceneManager.Instance.tracersEffectsPool;
			bulletHolesPool = GameSceneManager.Instance.bulletHolePool;
		}

		private void PlayerDeath()
		{
			CancelInvoke(nameof(ClientCallServerShoot));
		}

		[Client]
		internal void ShootWeapon(bool buttonDown)
		{
			//Cache our current weapon
			NetworkedWeapon networkedWeapon = weaponManager.GetActiveWeapon();

			//Looks like the weapon isn't setup yet
			if (networkedWeapon == null)
				return;

			if(networkedWeapon.IsReloading)
				return;

			TCWeapon weapon = networkedWeapon.GetTCWeapon();

			if (weapon == null)
				return;

			if(buttonDown && weapon.fireMode == TCWeapon.WeaponFireMode.Semi)
				ClientCallServerShoot();
			if(buttonDown && weapon.fireMode == TCWeapon.WeaponFireMode.Auto)
				InvokeRepeating(nameof(ClientCallServerShoot), 0f, 1f / weapon.fireRate);
			if(!buttonDown && weapon.fireMode == TCWeapon.WeaponFireMode.Auto)
				CancelInvoke(nameof(ClientCallServerShoot));
		}

		private void ClientCallServerShoot()
		{
			CmdShootWeapon();
		}

		#endregion

		#region Server Side Shooting

		/// <summary>
		///     Asks the server to shoot this client's weapon
		/// </summary>
		[Command(channel = Channels.Unreliable)]
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
			
			//If our clip is empty, then reload
			if (activeWeapon.CurrentBulletAmount <= 0)
			{
				//Reload
				weaponManager.StartReloadPlayerWeapon();
				return;
			}

			//If we are currently reloading, and we still got bullets left, cancel the reload
			if (activeWeapon.IsReloading)
			{
				if(activeWeapon.CurrentBulletAmount == 0)
					return;
				
				weaponManager.CancelReload();
				activeWeapon.IsReloading = false;
			}
			
			//If our player is dead, or we haven't reached our next time to fire, then return
			if (playerManager.IsDead || Time.time < nextTimeToFire)
				return;

			ServerShootWeapon(activeWeapon);

			//Update weapon stats
			weaponManager.TargetSendWeaponStatus(netIdentity.connectionToClient, activeWeapon);
		}

		/// <summary>
		///     Bang bang
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
		///     Does a ray cast based of the weapon properties
		/// </summary>
		[Server]
		private void WeaponRayCast()
		{
			//Next, get what weapon the player was using
			TCWeapon tcWeapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			if (tcWeapon == null)
				return;

			Stopwatch stopwatch = Stopwatch.StartNew();

			//Get the direction the player was facing
			Transform playerFacingDirection = localPlayerCamera;

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
					if (hit.collider.name == transform.name)
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
			RpcDoWeaponShootEffects(new WeaponShootEffectsTargets
			{
				Targets = targets.ToArray(),
				TargetNormals = targetsNormal.ToArray()
			});

			stopwatch.Stop();
			Logger.Debug("Took {@Milliseconds}ms to fire {@Player}'s {@Weapon}", stopwatch.Elapsed.TotalMilliseconds,
				transform.name, tcWeapon.weapon);
		}

		#endregion

		#region Weapon Effects

		/// <summary>
		///     Makes the muzzle flash play
		/// </summary>
		[ClientRpc(channel = Channels.Unreliable)]
		private void RpcWeaponMuzzleFlash()
		{
			weaponManager.GetActiveWeaponGraphics().muzzleFlash.Play(true);
		}

		/// <summary>
		///     Make a tracer effect go to the target
		/// </summary>
		/// <param name="hitTargets"></param>
		[ClientRpc(channel = Channels.Unreliable)]
		private void RpcDoWeaponShootEffects(WeaponShootEffectsTargets hitTargets)
		{
			TCWeapon weapon = weaponManager.GetActiveWeapon().GetTCWeapon();
			WeaponGraphics weaponGraphics = weaponManager.GetActiveWeaponGraphics();

			for (int i = 0; i < hitTargets.Targets.Length; i++)
			{
				//Do bullet tracer
				GameObject tracerObject = tracerPool.GetPooledObject();
				tracerObject.transform.position = weaponGraphics.bulletTracerPosition.position;
				tracerObject.transform.rotation = weaponGraphics.bulletTracerPosition.rotation;
				
				BulletTracer tracer = tracerObject.GetComponent<BulletTracer>();
				tracer.Play(hitTargets.Targets[i]);

				//Do bullet holes
				GameObject bulletHole = bulletHolesPool.GetPooledObject();
				bulletHole.transform.position = hitTargets.Targets[i];
				bulletHole.transform.rotation = Quaternion.LookRotation(hitTargets.TargetNormals[i]);
			}
		}

		private struct WeaponShootEffectsTargets : NetworkMessage
		{
			public Vector3[] Targets;

			public Vector3[] TargetNormals;
		}

		#endregion
	}
}