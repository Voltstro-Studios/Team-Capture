namespace Weapons
{
	/// <summary>
	/// A simplified version of <see cref="TCWeapon"/>, designed for sending over the network (as the name might implied)
	/// </summary>
	public class NetworkedWeapon : IWeaponBase
	{
		/// <summary>
		/// Creates a new <see cref="NetworkedWeapon"/>. Designed to send over the network
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="setMaxBullets"></param>
		public NetworkedWeapon(TCWeapon weapon, bool setMaxBullets = true)
		{
			associatedTCWeapon = weapon;
			Weapon = weapon.weapon;

			if(setMaxBullets)
				Reload();
		}

		/// <summary>
		/// What weapon is this?
		/// </summary>
		public string Weapon;

		/// <summary>
		/// How many bullets are currently in this gun?
		/// </summary>
		public int CurrentBulletAmount;

		/// <summary>
		/// Is this weapon reloading?
		/// </summary>
		public bool IsReloading;

		private readonly TCWeapon associatedTCWeapon;

		/// <summary>
		/// Gets the associated <see cref="TCWeapon"/> with this weapon type
		/// </summary>
		/// <returns></returns>
		public TCWeapon GetTCWeapon()
		{
			return associatedTCWeapon;
		}

		/// <inheritdoc/>
		public void Reload()
		{
			CurrentBulletAmount = associatedTCWeapon.maxBullets;
			IsReloading = false;
		}
	}
}