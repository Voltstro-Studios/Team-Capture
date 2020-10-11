using Mirror;

namespace Core.Networking.Messages
{
	/// <summary>
	/// A message for initially sending the status of all the pickups
	/// </summary>
	public struct InitPickupStatusMessage : NetworkMessage
	{
		/// <summary>
		/// All the disabled pickups
		/// </summary>
		public string[] DisabledPickups;
	}
}