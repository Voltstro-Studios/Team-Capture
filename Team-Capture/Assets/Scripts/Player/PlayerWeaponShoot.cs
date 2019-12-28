using Mirror;
using UnityEngine;
using Weapons;

namespace Player
{
	public class PlayerWeaponShoot : NetworkBehaviour
	{
		private WeaponManager weaponManager;
		private PlayerManager playerManager;

		private void Start()
		{
			weaponManager = GetComponent<WeaponManager>();
			playerManager = GetComponent<PlayerManager>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			//Cache our current weapon
			TCWeapon weapon = GetCurrentWeapon();

			//Looks like the weapon isn't setup yet
			if (weapon == null)
				return;

			if (playerManager.IsDead)
			{
				CancelInvoke(nameof(ShootWeapon));
				return;
			}

			if (weapon.currentBulletsAmount < weapon.maxBullets && !weapon.isReloading)
				//Do a reload
				if (Input.GetButtonDown("Reload"))
				{
					CancelInvoke(nameof(ShootWeapon));
					weaponManager.StartCoroutine(weaponManager.ReloadCurrentWeapon());
					return;
				}

			//Semi-automatic weapons
			if (weapon.fireRate <= 0f)
			{
				//Only shoot if the player just pressed the button
				if (Input.GetButtonDown("Fire1") && !weapon.isReloading) ShootWeapon();
			}
			//Full auto weapons
			else
			{
				if (Input.GetButtonDown("Fire1") && !weapon.isReloading)
					InvokeRepeating(nameof(ShootWeapon), 0f, 1f / weapon.fireRate);
				else if (Input.GetButtonUp("Fire1")) CancelInvoke(nameof(ShootWeapon));
			}
		}

		[Client]
		private void ShootWeapon()
		{
			if (!isLocalPlayer)
				return;

			TCWeapon weapon = GetCurrentWeapon();

			if (weapon.currentBulletsAmount <= 0)
			{
				weaponManager.StartCoroutine(weaponManager.ReloadCurrentWeapon());
				playerManager.clientUi.hud.UpdateAmmoUi(weaponManager);
				return;
			}

			playerManager.clientUi.hud.UpdateAmmoUi(weaponManager);

			weapon.currentBulletsAmount--;

			CmdWeaponMuzzleFlash(transform.name);

			CmdWeaponRayCast(transform.name, weapon.weapon,
				GetComponent<PlayerSetup>().GetPlayerCamera().transform.position,
				GetComponent<PlayerSetup>().GetPlayerCamera().transform.forward);
		}

		[Command]
		private void CmdWeaponRayCast(string sourcePlayer, string weapon, Vector3 origin, Vector3 direction)
		{
			//TODO: Do lag compensation

			TCWeapon tcWeapon = TCWeaponsManager.GetWeapon(weapon);
			if (tcWeapon == null)
				return;

			// ReSharper disable once Unity.PreferNonAllocApi
			RaycastHit[] hits = Physics.RaycastAll(origin, direction, tcWeapon.range);
			bool hitPlayer = false;

			foreach (RaycastHit hit in hits)
			{
				//If we have already hit a player, return
				if (hitPlayer)
					return;

				//If the hit was the sourcePlayer, then ignore it
				if (hit.collider.name == sourcePlayer)
					continue;

				RpcWeaponImpact(hit.point, hit.normal);

				if (hit.collider.GetComponent<PlayerManager>() == null) continue;

				hit.collider.GetComponent<PlayerManager>().RpcTakeDamage(sourcePlayer, tcWeapon.damage);
				hitPlayer = true;
			}
		}

		private TCWeapon GetCurrentWeapon()
		{
			return weaponManager.GetActiveWeapon();
		}

		#region Weapon Effects

		#region Weapon Muzzle

		[Command]
		private void CmdWeaponMuzzleFlash(string playerId)
		{
			RpcWeaponMuzzleFlash(playerId);
		}

		[ClientRpc]
		private void RpcWeaponMuzzleFlash(string playerId)
		{
			PlayerManager player = GameManager.GetPlayer(playerId);
			if (player == null) return;

			player.GetComponent<WeaponManager>().GetActiveWeaponGraphics().muzzleFlash.Play(true);
		}

		#endregion

		#region Weapon Impact

		[ClientRpc]
		private void RpcWeaponImpact(Vector3 pos, Vector3 normal)
		{
			GameObject hitEffect =
				Instantiate(GameManager.Instance.scene.weaponHit, pos, Quaternion.LookRotation(normal));
			Destroy(hitEffect, GameManager.Instance.scene.hitObjectLastTime);
		}

		#endregion

		#endregion
	}
}