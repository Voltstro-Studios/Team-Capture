// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     What type of weapon is this
    /// </summary>
    public enum WeaponType : byte
    {
        /// <summary>
        ///     'Default' ray-casting gun
        /// </summary>
        Default,

        /// <summary>
        ///     A Melee weapon
        /// </summary>
        Melee,

        /// <summary>
        ///     Shoots a physical object
        /// </summary>
        Projectile
    }
}