using Mirror;
using Team_Capture.Logging;

namespace Team_Capture.Weapons
{
	/// <summary>
	///     A <see cref="NetworkWriter" /> and <see cref="NetworkReader" /> for <see cref="NetworkedWeapon" />
	/// </summary>
	internal static class NetworkedWeaponReaderWriter
	{
		/// <summary>
		///     Writes a <see cref="NetworkedWeapon" />
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="weapon"></param>
		public static void WriteNetworkedWeapon(this NetworkWriter writer, NetworkedWeapon weapon)
		{
			writer.WriteString(weapon.Weapon);
			writer.WriteInt(weapon.CurrentBulletAmount);
			writer.WriteBool(weapon.IsReloading);
		}

		/// <summary>
		///     Reads a <see cref="NetworkedWeapon" />
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static NetworkedWeapon ReadNetworkedWeapon(this NetworkReader reader)
		{
			//First, read the weapon
			TCWeapon weapon = WeaponsResourceManager.GetWeapon(reader.ReadString());

			//Return the NetworkedWeapon
			if (weapon != null)
				return new NetworkedWeapon(weapon, false)
				{
					CurrentBulletAmount = reader.ReadInt(),
					IsReloading = reader.ReadBool()
				};

			//Something went wrong
			Logger.Error("Sent networked weapon doesn't have a TCWeapon!");
			return null;
		}
	}
}