using UnityEngine;
using Mirror;
using Weapons;

namespace Player
{
    public class PlayerWeaponShoot : NetworkBehaviour
    {
        private WeaponManager weaponManager;

        private void Start()
        {
            weaponManager = GetComponent<WeaponManager>();
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

            if (GetComponent<PlayerManager>().IsDead)
            {
                CancelInvoke(nameof(ShootWeapon));
                return;
            }

            if (weapon.currentBulletsAmount < weapon.maxBullets && !weapon.isReloading)
            {
                //Do a reload
                if (Input.GetButtonDown("Reload"))
                {
                    CancelInvoke(nameof(ShootWeapon));
                    weaponManager.StartCoroutine(weaponManager.ReloadCurrentWeapon());
                    return;
                }
            }
            //Semi-automatic weapons
            if (weapon.fireRate <= 0f)
            {
                //Only shoot if the player just pressed the button
                if (Input.GetButtonDown("Fire1") && !weapon.isReloading)
                {
                    ShootWeapon();
                }
            }
            //Full auto weapons
            else
            {
                if (Input.GetButtonDown("Fire1") && !weapon.isReloading)
                {
                    InvokeRepeating(nameof(ShootWeapon), 0f, 1f / weapon.fireRate);
                }
                else if (Input.GetButtonUp("Fire1"))
                {
                    CancelInvoke(nameof(ShootWeapon));
                }
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
                return;
            }

            weapon.currentBulletsAmount--;

            CmdWeaponMuzzleFlash(transform.name);

            WeaponRayCast(transform.name, weapon.weapon, GetComponent<PlayerSetup>().GetPlayerCamera().transform.position, GetComponent<PlayerSetup>().GetPlayerCamera().transform.forward);
        }

        private void WeaponRayCast(string sourcePlayer, string weapon, Vector3 orgin, Vector3 direction)
        {
	        //We would do the weapon ray cast on the server, but you need to do lag compensation.
	        //Since the client and server are basically never synced(E.G: Player location), so when we tell the server
	        //to do a ray cast, what the server sees will be different to what the client sees
	        //TODO: Handle ray cast on the server once we have lag compensation

	        TCWeapon tcWeapon = TCWeaponsManager.GetWeapon(weapon);
			if(tcWeapon == null)
				return;

			// ReSharper disable once Unity.PreferNonAllocApi
			RaycastHit[] hits = Physics.RaycastAll(orgin, direction, tcWeapon.range);
			bool hitPlayer = false;

			foreach (RaycastHit hit in hits)
			{
				//If we have already hit a player, return
				if(hitPlayer)
					return;

				//If the hit was the sourcePlayer, then ignore it
				if(hit.collider.name == sourcePlayer)
					continue;

				CmdWeaponImpact(hit.point, hit.normal);

				if (hit.collider.GetComponent<PlayerManager>() == null) continue;

				hit.collider.GetComponent<PlayerManager>().CmdTakeDamage(sourcePlayer, tcWeapon.damage);
				hitPlayer = true;
			}
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

		[Command]
		private void CmdWeaponImpact(Vector3 pos, Vector3 normal)
        {
	        RpcWeaponImpact(pos, normal);
        }

        [ClientRpc]
        private void RpcWeaponImpact(Vector3 pos, Vector3 normal)
        {
	       GameObject hitEffect = Instantiate(GameManager.Instance.scene.weaponHit, pos, Quaternion.LookRotation(normal));
		   Destroy(hitEffect, GameManager.Instance.scene.hitObjectLastTime);
        }

        #endregion

        #endregion

        private TCWeapon GetCurrentWeapon()
        {
            return weaponManager.GetActiveWeapon();
        }
    }
}