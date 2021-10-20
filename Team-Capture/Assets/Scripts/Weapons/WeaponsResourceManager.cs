// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Linq;
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
		private static IList<TCWeapon> weapons;

		private const string WeaponLabel = "Weapon";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			weapons = Addressables.LoadAssetsAsync<TCWeapon>(WeaponLabel, null).WaitForCompletion();
			Logger.Debug("Loaded {WeaponCount} weapons.", weapons.Count);
		}

		/// <summary>
		///     Gets a <see cref="TCWeapon" />
		/// </summary>
		/// <param name="weaponName"></param>
		/// <returns></returns>
		public static TCWeapon GetWeapon(string weaponName)
		{
			return weapons.FirstOrDefault(w => w.weapon == weaponName);
		}
	}
}