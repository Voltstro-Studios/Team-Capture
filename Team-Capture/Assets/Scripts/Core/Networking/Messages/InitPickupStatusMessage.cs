using Mirror;

namespace Team_Capture.Core.Networking.Messages
{
	/// <summary>
	///     A message for initially sending the status of all the pickups
	/// </summary>
	internal struct InitPickupStatusMessage : NetworkMessage
	{
		/// <summary>
		///     All the disabled pickups
		/// </summary>
		public string[] DisabledPickups;
	}
}