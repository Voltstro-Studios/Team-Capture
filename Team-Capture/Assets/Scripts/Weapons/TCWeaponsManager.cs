using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapons
{
	public static class TCWeaponsManager
	{
		public static IEnumerable<TCWeapon> GetAllTCWeapons()
		{
			return Resources.LoadAll<TCWeapon>("");
		}

		public static TCWeapon GetWeapon(string weaponName)
		{
			return GetAllTCWeapons().FirstOrDefault(w => w.weapon == weaponName);
		}
	}
}