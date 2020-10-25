using Mirror;

namespace Core.Networking.Messages
{
	internal struct SetPickupStatus : NetworkMessage
	{
		public string PickupName;
		public bool IsActive;
	}
}