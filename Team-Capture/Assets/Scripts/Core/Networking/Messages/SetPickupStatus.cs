using Mirror;

namespace Team_Capture.Core.Networking.Messages
{
	internal struct SetPickupStatus : NetworkMessage
	{
		public string PickupName;
		public bool IsActive;
	}
}