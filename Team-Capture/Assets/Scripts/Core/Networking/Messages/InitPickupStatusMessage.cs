using Mirror;

namespace Core.Networking.Messages
{
	/// <summary>
	/// A message for initially sending the status of all the pickups
	/// </summary>
	public struct InitPickupStatusMessage : IMessageBase
	{
		/// <summary>
		/// All the disabled pickups
		/// </summary>
		public string[] DisabledPickups;

		public void Deserialize(NetworkReader reader) { }

		public void Serialize(NetworkWriter writer) { }
	}
}