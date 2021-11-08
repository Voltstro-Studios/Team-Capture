// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using NetFabric.Hyperlinq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons
{
    /// <summary>
    ///     Handles getting weapon data
    /// </summary>
    public static class WeaponsResourceManager
    {
        private const string WeaponLabel = "Weapon";
        private static IList<WeaponBase> weapons;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            weapons = Addressables.LoadAssetsAsync<WeaponBase>(WeaponLabel, null).WaitForCompletion();
            Logger.Debug("Loaded {WeaponCount} weapons.", weapons.Count);
        }

        /// <summary>
        ///     Gets a <see cref="WeaponBase" />
        /// </summary>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        public static WeaponBase GetWeapon(string weaponId)
        {
            return weapons.AsValueEnumerable()
                .Where(w => w.weaponId == weaponId)
                .First().Value;
        }
    }
}