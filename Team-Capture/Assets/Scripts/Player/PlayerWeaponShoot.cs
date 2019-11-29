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
			if(!isLocalPlayer)
				return;

			//Looks like the weapon isn't setup yet
			if(GetCurrentWeapon() == null)
				return;

			if (GetComponent<PlayerManager>().IsDead)
			{
				CancelInvoke(nameof(ShootWeapon));
				return;
			}

			if(GetCurrentWeapon().currentBulletsAmount < GetCurrentWeapon().maxBullets && !GetCurrentWeapon().isReloading)
			{
				//Do a reload
				if (Input.GetButtonDown("Reload"))
				{
					CancelInvoke(nameof(ShootWeapon));
					weaponManager.StartCoroutine(nameof(WeaponManager.ReloadCurrentWeapon));
					return;
				}
			}

			if(GetCurrentWeapon().fireRate <= 0f)
			{
				if (Input.GetButtonDown("Fire1") && !GetCurrentWeapon().isReloading)
				{
					ShootWeapon();
				}
			}
			else
			{
				if(Input.GetButtonDown("Fire1") && !GetCurrentWeapon().isReloading)
				{
					InvokeRepeating(nameof(ShootWeapon), 0f, 1f/GetCurrentWeapon().fireRate);
				}
				else if(Input.GetButtonUp("Fire1"))
				{
					CancelInvoke(nameof(ShootWeapon));
				}
			}
		}
		
		[Client]
		private void ShootWeapon()
		{
			if(!isLocalPlayer)
				return;

			if (GetCurrentWeapon().currentBulletsAmount <= 0)
			{
				weaponManager.StartCoroutine(nameof(WeaponManager.ReloadCurrentWeapon));
				return;
			}

			GetCurrentWeapon().currentBulletsAmount--;

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
			if(player == null) return;

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
