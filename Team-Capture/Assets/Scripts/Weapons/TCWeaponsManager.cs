using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapons
{
	public static class TCWeaponsManager
	{
		public static IEnumerable<TCWeapon> GetAllTCWeapons() => Resources.LoadAll<TCWeapon>("");

		public static TCWeapon GetWeapon(string weaponName) => GetAllTCWeapons().FirstOrDefault(w => w.weapon == weaponName);
	}
}
