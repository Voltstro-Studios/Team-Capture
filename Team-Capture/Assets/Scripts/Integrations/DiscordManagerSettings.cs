using Discord.GameSDK;
using UnityEngine;

namespace Integrations
{
	[System.Serializable]
	internal class DiscordManagerSettings
	{
		/// <summary>
		/// The client ID that we will use
		/// </summary>
		[Tooltip("The client ID that we will use"), SerializeField]
		public string clientId;

		/// <summary>
		/// The default game detail message
		/// </summary>
		[Tooltip("The default game detail message")]
		public string defaultGameDetail = "Loading...";

		/// <summary>
		/// The default game state message
		/// </summary>
		[Tooltip("The default game state message")]
		public string defaultGameState = "Loading...";

		/// <summary>
		/// The default large image to use
		/// </summary>
		[Tooltip("The default large image to use")]
		public string defaultLargeImage = "tc_icon";

		/// <summary>
		/// The log level to use
		/// </summary>
		[Tooltip("The log level to use")]
		public LogLevel logLevel = LogLevel.Warn;
	}
}