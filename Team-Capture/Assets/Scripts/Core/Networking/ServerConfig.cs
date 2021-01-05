using System;
using Mirror;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///		Config for server settings
	/// </summary>
	[Serializable]
	internal struct ServerConfig : NetworkMessage
	{
		/// <summary>
		///		The name of the game
		/// </summary>
		public string gameName;
	}
}