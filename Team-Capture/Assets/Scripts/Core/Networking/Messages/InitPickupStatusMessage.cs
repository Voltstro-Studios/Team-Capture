using Mirror;

namespace Core.Networking.Messages
{
	/// <summary>
	/// A message for initially sending the status of all the pickups
	/// </summary>
	public class InitPickupStatusMessage : MessageBase
	{
		/// <summary>
		/// All the disabled pickups
		/// </summary>
		public string[] DisabledPickups;
	}
}