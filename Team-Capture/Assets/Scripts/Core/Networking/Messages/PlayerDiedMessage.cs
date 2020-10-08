using Mirror;

namespace Core.Networking.Messages
{
	/// <summary>
	/// A player has died message
	/// </summary>
	public struct PlayerDiedMessage : IMessageBase
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

		public void Deserialize(NetworkReader reader) { }

		public void Serialize(NetworkWriter writer) { }
	}
}