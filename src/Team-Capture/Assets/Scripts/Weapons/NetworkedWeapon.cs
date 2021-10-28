// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     A simplified version of <see cref="TCWeapon" />, designed for sending over the network (as the name might implied)
    /// </summary>
    public class NetworkedWeapon : IWeaponBase
    {
        private readonly TCWeapon associatedTCWeapon;

        /// <summary>
        ///     How many bullets are currently in this gun?
        /// </summary>
        public int CurrentBulletAmount;

        /// <summary>
        ///     Is this weapon reloading?
        /// </summary>
        public bool IsReloading;

        /// <summary>
        ///     What weapon is this?
        /// </summary>
        public string Weapon;

        /// <summary>
        ///     Creates a new <see cref="NetworkedWeapon" />. Designed to send over the network
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="setMaxBullets"></param>
        public NetworkedWeapon(TCWeapon weapon, bool setMaxBullets = true)
        {
            associatedTCWeapon = weapon;
            Weapon = weapon.weapon;

            if (setMaxBullets)
                Reload(true);
        }

        /// <inheritdoc />
        public void Reload(bool setMaxBullets = false)
        {
            if (setMaxBullets || associatedTCWeapon.reloadMode == TCWeapon.WeaponReloadMode.Clip)
                CurrentBulletAmount = associatedTCWeapon.maxBullets;

            IsReloading = false;
        }

        /// <summary>
        ///     Gets the associated <see cref="TCWeapon" /> with this weapon type
        /// </summary>
        /// <returns></returns>
        public TCWeapon GetTCWeapon()
        {
            return associatedTCWeapon;
        }
    }
}