using Mirror;

namespace Core.Networking.Messages
{
	public class WeaponSyncMessage : MessageBase
	{
		public int CurrentBullets;
		public bool IsReloading;
	}
}
