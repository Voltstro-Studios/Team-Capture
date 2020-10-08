using Mirror;

namespace Core.Networking.Messages
{
	public struct SetPickupStatus : IMessageBase
	{
		public string PickupName;
		public bool IsActive;

		public void Deserialize(NetworkReader reader) { }

		public void Serialize(NetworkWriter writer) { }
	}
}