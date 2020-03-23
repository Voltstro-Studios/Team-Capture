namespace Weapons
{
	/// <summary>
	/// A simplified version of <see cref="TCWeapon"/>, designed for sending over the network (as the name might implied)
	/// </summary>
	public class NetworkedWeapon
	{
		public string weapon;
		public int currentBulletAmount;
		public bool IsReloading;

		internal TCWeapon GetTCWeapon()
		{
			return WeaponsResourceManager.GetWeapon(weapon);
		}
	}
}
