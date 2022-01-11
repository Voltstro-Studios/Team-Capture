// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Player;
using Team_Capture.Weapons;
using UnityEngine;

namespace Team_Capture.Pickups
{
    internal class WeaponPickup : Pickup
    {
        /// <summary>
        ///     The weapon to give
        /// </summary>
        [Tooltip("The weapon to give")] public WeaponBase weapon;

        protected override void OnPlayerPickup(PlayerManager player)
        {
            WeaponManager weaponManager = player.GetComponent<WeaponManager>();

            //Don't want to pickup the same weapon
            if (weaponManager.GetWeaponFromId(weapon.weaponId) != null)
                return;

            weaponManager.AddWeapon(weapon);

            base.OnPlayerPickup(player);
        }
    }
}