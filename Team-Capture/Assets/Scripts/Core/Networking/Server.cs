using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Mirror;
using Team_Capture.Core.Logging;

namespace Team_Capture.Core.Networking
{
	public static class Server
	{
		private const string ServerOnlineFile = "SERVERONLINE";
		private const string ServerOnlineFileMessage = "A -Gawr Gura";

		private static FileStream serverOnlineFileStream;

		/// <summary>
		///		Call this when the server is started
		/// </summary>
		public static void OnStartServer()
		{
			string serverOnlinePath = $"{Game.GetGameExecutePath()}/{ServerOnlineFile}";

			if (File.Exists(serverOnlinePath))
				throw new Exception("Server is already online!");

			serverOnlineFileStream = File.Create(serverOnlinePath, 128, FileOptions.DeleteOnClose);
			byte[] messageBytes = Encoding.UTF8.GetBytes(ServerOnlineFileMessage);
			serverOnlineFileStream.Write(messageBytes, 0, messageBytes.Length);
			serverOnlineFileStream.Flush();
			File.SetAttributes(serverOnlinePath, FileAttributes.Hidden);
		}

		/// <summary>
		///		Call this when the server is stopped
		/// </summary>
		public static void OnStopServer()
		{
			serverOnlineFileStream.Close();
			serverOnlineFileStream.Dispose();
			serverOnlineFileStream = null;
		}

		public static void CreateServerAndConnectToServer(this NetworkManager netManager, string gameName, string sceneName, int maxPlayers)
		{
#if UNITY_EDITOR
			string serverOnlinePath =
				$"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/{ServerOnlineFile}";
#else
			string serverOnlinePath = $"{Game.GetGameExecutePath()}/{ServerOnlineFile}";
#endif

			if (File.Exists(serverOnlinePath))
			{
				Logger.Error("A server is already running!");
				return;
			}

			Process newTcServer = new Process
			{
				StartInfo = new ProcessStartInfo
				{
#if UNITY_EDITOR
					FileName =
						$"{Voltstro.UnityBuilder.Build.GameBuilder.GetBuildDirectory()}Team-Capture-Quick/Team-Capture.exe",
#elif UNITY_STANDALONE_WIN
					FileName = "Team-Capture.exe",
#else
					FileName = "Team-Capture",
#endif
					Arguments =
						$"-batchmode -nographics -gamename \"{gameName}\" -scene {sceneName} -maxplayers {maxPlayers}"
				}
			};
			newTcServer.Start();

			while (!File.Exists(serverOnlinePath))
			{
			}

			netManager.networkAddress = "localhost";
			netManager.StartClient();
		}
	}
}