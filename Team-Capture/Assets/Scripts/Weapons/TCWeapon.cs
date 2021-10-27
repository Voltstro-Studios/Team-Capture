// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Weapons
{
	/// <summary>
	///     A weapon for Team-Capture
	/// </summary>
	[CreateAssetMenu(fileName = "New TC Weapon", menuName = "Team-Capture/TCWeapon")]
	public class TCWeapon : ScriptableObject
	{
		/// <summary>
		///     A weapon's fire mode
		/// </summary>
		public enum WeaponFireMode
		{
			/// <summary>
			///     An automatic firearm is a firearm capable of automatically cycling the shooting process,
			///     without needing any more manual operation from the user than simply actuating a trigger.
			/// </summary>
			Auto,

			/// <summary>
			///     A semi-automatic firearm, also called self-loading firearm or autoloading firearm
			///     (though fully automatic and selective fire firearms are also self-loading), is one whose action
			///     mechanism automatically loads a following round of cartridge into the chamber (self-loading) and
			///     prepares it for subsequent firing, but requires the shooter to manually actuate the trigger in order to discharge
			///     each
			///     shot.
			/// </summary>
			Semi
		}
		
		/// <summary>
		///		How to reload a weapon
		/// </summary>
		public enum WeaponReloadMode : byte
		{
			/// <summary>
			///		Reload the entire clip each time
			/// </summary>
			Clip,
			
			/// <summary>
			///		Reload each shell individually
			/// </summary>
			Shells
		}

		/// <summary>
		///     The weapon's name, this will be sent across networks so make it short
		/// </summary>
		[Header("Base Weapon Settings")]
		[Tooltip("The weapon's name, this will be sent across networks so make it short")]
		public string weapon;

		/// <summary>
		///     The formatted name. This is what will show on HUDs
		/// </summary>
		[Tooltip("The formatted name. This is what will show on HUDs")] [SerializeField]
		private string weaponFormattedName;

		/// <summary>
		///     The prefab that this weapon will use
		/// </summary>
		[Tooltip("The prefab that this weapon will use")]
		public GameObject baseWeaponPrefab;

		/// <summary>
		///     How much damage per bullet will this weapon do
		/// </summary>
		[Header("Weapon Stats")] [Tooltip("How much damage per bullet will this weapon do")]
		public int damage;

		/// <summary>
		///     The rate at witch this weapon will fire at
		/// </summary>
		[Tooltip("The rate at witch this weapon will fire at")]
		public float fireRate;

		/// <summary>
		///     The fire-mode that this weapon will use
		/// </summary>
		[Tooltip("The fire-mode that this weapon will use")]
		public WeaponFireMode fireMode;

		/// <summary>
		///		The reload-mode that this weapon will use
		/// </summary>
		[Tooltip("The reload-mode that this weapon will use")]
		public WeaponReloadMode reloadMode;

		/// <summary>
		///     How many bullets will come out per shot
		/// </summary>
		[Tooltip("How many bullets will come out per shot")]
		public int bulletsPerShot = 1;

		/// <summary>
		///     The max amount of bullets per maz
		/// </summary>
		[Tooltip("The max amount of bullets per maz")]
		public int maxBullets;

		/// <summary>
		///     The range at witch the raycast will go
		/// </summary>
		[Tooltip("The range at witch the raycast will go")]
		public int range;

		/// <summary>
		///     The time it takes to reload the weapon
		/// </summary>
		[Tooltip("The time it takes to reload the weapon")]
		public int reloadTime = 2000;

		/// <summary>
		///     How much spread will this weapon have
		/// </summary>
		[Header("Spread")] [Tooltip("How much spread will this weapon have")]
		public float spreadFactor = 0.05f;
	}
}