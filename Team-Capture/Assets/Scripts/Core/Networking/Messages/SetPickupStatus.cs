using Mirror;

namespace Core.Networking.Messages
{
	public class SetPickupStatus : MessageBase
	{
		public string PickupName;
		public bool IsActive;
	}
}
