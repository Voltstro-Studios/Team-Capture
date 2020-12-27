using System;
using System.IO;
using System.Text;

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
	}
}