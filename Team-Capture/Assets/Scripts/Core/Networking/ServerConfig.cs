using System;
using Mirror;
using UnityEngine.Scripting;

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

		/// <summary>
		///		The MOTD mode
		/// </summary>
		public Server.ServerMOTDMode motdMode;

		/// <summary>
		///		Text for MOTD
		/// </summary>
		public string motdText;

		/// <summary>
		///		URL for MOTD
		/// </summary>
		public string motdUrl;
	}

	[Preserve]
	internal static class ServerConfigNetwork
	{
		public static void WriteServerConfig(this NetworkWriter writer, ServerConfig config)
		{
			writer.WriteString(config.gameName);
			writer.WriteByte((byte)config.motdMode);
			if(config.motdMode == Server.ServerMOTDMode.TextOnly)
				writer.WriteString(config.motdText);
			else if(config.motdMode == Server.ServerMOTDMode.WebOnly)
				writer.WriteString(config.motdUrl);
			else if (config.motdMode == Server.ServerMOTDMode.WebWithTextBackup)
			{
				writer.WriteString(config.motdText);
				writer.WriteString(config.motdUrl);
			}
		}

		public static ServerConfig ReadServerConfig(this NetworkReader reader)
		{
			ServerConfig config = new ServerConfig
			{
				gameName = reader.ReadString(),
				motdMode = (Server.ServerMOTDMode)reader.ReadByte()
			};

			if (config.motdMode == Server.ServerMOTDMode.TextOnly)
				config.motdText = reader.ReadString();
			else if (config.motdMode == Server.ServerMOTDMode.WebOnly)
				config.motdUrl = reader.ReadString();
			else if (config.motdMode == Server.ServerMOTDMode.WebWithTextBackup)
			{
				config.motdText = reader.ReadString();
				config.motdUrl = reader.ReadString();
			}

			return config;
		}
	}
}