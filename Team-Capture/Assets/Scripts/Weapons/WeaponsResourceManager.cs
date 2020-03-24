using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapons
{
	/// <summary>
	/// Handles getting weapon data
	/// </summary>
	public static class WeaponsResourceManager
	{
		private static List<TCWeapon> weapons;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			weapons = GetAllTCWeapons().ToList();
			//Logger.Log("Got all weapons", LogVerbosity.Debug);
		}

		private static IEnumerable<TCWeapon> GetAllTCWeapons()
		{
			return Resources.LoadAll<TCWeapon>("");
		}

		/// <summary>
		/// Gets a <see cref="TCWeapon"/>
		/// </summary>
		/// <param name="weaponName"></param>
		/// <returns></returns>
		public static TCWeapon GetWeapon(string weaponName)
		{
			return weapons.FirstOrDefault(w => w.weapon == weaponName);
		}
	}
}