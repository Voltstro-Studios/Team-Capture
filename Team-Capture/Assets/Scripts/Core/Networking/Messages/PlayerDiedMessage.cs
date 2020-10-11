using Mirror;

namespace Core.Networking.Messages
{
	/// <summary>
	/// A player has died message
	/// </summary>
	public struct PlayerDiedMessage : NetworkMessage
	{
		/// <summary>
		/// Who was the victim
		/// </summary>
		public string PlayerKilled;

		/// <summary>
		/// Who was the murderer
		/// </summary>
		public string PlayerKiller;

		/// <summary>
		/// What weapon did they use
		/// </summary>
		public string WeaponName;
	}
}