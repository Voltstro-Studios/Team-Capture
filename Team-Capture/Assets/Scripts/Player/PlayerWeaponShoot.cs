using System;
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

			if(Input.GetButtonDown("Fire1"))
				ShootWeapon();
		}

		private void ShootWeapon()
		{
			Debug.Log("Shoot");

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
