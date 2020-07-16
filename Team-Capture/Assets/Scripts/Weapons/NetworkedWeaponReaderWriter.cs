using Core.Logging;
using Mirror;

namespace Weapons
{
	public static class NetworkedWeaponReaderWriter
	{
		public static void WriteNetworkedWeapon(this NetworkWriter writer, NetworkedWeapon weapon)
		{
			writer.WriteString(weapon.Weapon);
			writer.WriteInt32(weapon.CurrentBulletAmount);
			writer.WriteBoolean(weapon.IsReloading);
		}

		public static NetworkedWeapon ReadNetworkedWeapon(this NetworkReader reader)
		{
			//First, read the weapon
			TCWeapon weapon = WeaponsResourceManager.GetWeapon(reader.ReadString());

			//Return the NetworkedWeapon
			if (weapon != null)
				return new NetworkedWeapon(weapon, false)
				{
					CurrentBulletAmount = reader.ReadInt32(),
					IsReloading = reader.ReadBoolean()
				};

			//Something went wrong
			Logger.Error("Sent networked weapon doesn't have a TCWeapon!");
			return null;
		}
	}
}