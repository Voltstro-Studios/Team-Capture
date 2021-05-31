// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Core.Compression;
using UnityEngine.Scripting;

namespace Team_Capture.Core.Networking
{
	/// <summary>
	///		Config for server settings
	/// </summary>
	internal struct ServerConfig : NetworkMessage
	{
		internal ServerConfig(string gameName, Server.ServerMOTDMode motdMode, string motdText, string motdUrl)
		: this()
		{
			if(gameName != null)
				GameName = new CompressedNetworkString(gameName);
			MotdMode = motdMode;
			if(motdText != null)
				MotdText = new CompressedNetworkString(motdText);
			if(motdUrl != null)
				MotdUrl = new CompressedNetworkString(motdUrl);
		}

		/// <summary>
		///		The name of this game
		/// </summary>
		public CompressedNetworkString GameName;

		/// <summary>
		///		The MOTD mode
		/// </summary>
		public Server.ServerMOTDMode MotdMode;

		/// <summary>
		///		Text for MOTD
		/// </summary>
		public CompressedNetworkString MotdText;

		/// <summary>
		///		URL for MOTD
		/// </summary>
		public CompressedNetworkString MotdUrl;
	}

	[Preserve]
	internal static class ServerConfigNetwork
	{
		public static void WriteServerConfig(this NetworkWriter writer, ServerConfig config)
		{
			config.GameName.Write(writer);
			writer.WriteByte((byte)config.MotdMode);
			if(config.MotdMode == Server.ServerMOTDMode.TextOnly)
				config.MotdText.Write(writer);
			else if(config.MotdMode == Server.ServerMOTDMode.WebOnly)
				config.MotdUrl.Write(writer);
			else if (config.MotdMode == Server.ServerMOTDMode.WebWithTextBackup)
			{
				config.MotdText.Write(writer);
				config.MotdUrl.Write(writer);
			}
		}

		public static ServerConfig ReadServerConfig(this NetworkReader reader)
		{
			ServerConfig config = new ServerConfig
			{
				GameName = CompressedNetworkString.Read(reader),
				MotdMode = (Server.ServerMOTDMode)reader.ReadByte()
			};

			if (config.MotdMode == Server.ServerMOTDMode.TextOnly)
				config.MotdText = CompressedNetworkString.Read(reader);
			else if (config.MotdMode == Server.ServerMOTDMode.WebOnly)
				config.MotdUrl = CompressedNetworkString.Read(reader);
			else if (config.MotdMode == Server.ServerMOTDMode.WebWithTextBackup)
			{
				config.MotdText = CompressedNetworkString.Read(reader);
				config.MotdUrl = CompressedNetworkString.Read(reader);
			}

			return config;
		}
	}
}