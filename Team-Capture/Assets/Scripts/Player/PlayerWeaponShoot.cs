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

        #endregion

        #endregion

        private TCWeapon GetCurrentWeapon()
        {
            return weaponManager.GetActiveWeapon();
        }
    }
}