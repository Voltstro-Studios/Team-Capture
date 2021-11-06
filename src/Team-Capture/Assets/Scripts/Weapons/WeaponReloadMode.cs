// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     How to reload a weapon
    /// </summary>
    public enum WeaponReloadMode : byte
    {
        /// <summary>
        ///     Reload the entire clip each time
        /// </summary>
        Clip,

        /// <summary>
        ///     Reload each shell individually
        /// </summary>
        Shells
    }
}