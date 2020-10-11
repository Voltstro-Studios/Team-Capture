using Mirror;

namespace Core.Networking.Messages
{
	public struct SetPickupStatus : NetworkMessage
	{
		public string PickupName;
		public bool IsActive;
	}
}