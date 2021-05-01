using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	public class TCAuthenticator : NetworkAuthenticator
	{
		#region Server

		public override void OnStartServer()
		{
			NetworkServer.RegisterHandler<JoinRequestMessage>(OnRequestJoin, false);
		}

		public override void OnStopServer()
		{
			NetworkServer.UnregisterHandler<JoinRequestMessage>();
		}

		public override void OnServerAuthenticate(NetworkConnection conn)
		{
		}

		private void OnRequestJoin(NetworkConnection conn, JoinRequestMessage msg)
		{
			if (msg.ApplicationVersion == Application.version)
			{
				conn.Send(new JoinRequestResponseMessage
				{
					Code = 200,
					Message = "Ok"
				});

				ServerAccept(conn);
			}
			else
			{
				conn.Send(new JoinRequestResponseMessage
				{
					Code = 412,
					Message = "Server and client versions are out of sync!"
				});

				conn.isAuthenticated = false;
				DisconnectClientDelayed(conn).GetAwaiter().GetResult();
			}
		}

		private async Task DisconnectClientDelayed(NetworkConnection conn)
		{
			await Task.Delay(1000);

			ServerReject(conn);
		}

		#endregion

		#region Client

		public override void OnStartClient()
		{
			NetworkClient.RegisterHandler<JoinRequestResponseMessage>(OnReceivedJoinRequestResponse, false);
		}

		public override void OnStopClient()
		{
			NetworkClient.UnregisterHandler<JoinRequestResponseMessage>();
		}

		public override void OnClientAuthenticate()
		{
			NetworkClient.connection.Send(new JoinRequestMessage
			{
				ApplicationVersion = Application.version
			});
		}

		private void OnReceivedJoinRequestResponse(JoinRequestResponseMessage msg)
		{
			//We good to connect
			if (msg.Code == 200)
			{
				Logger.Info("Join request was accepted! {@Message} ({@Code})", msg.Message, msg.Code);

				ClientAccept();
			}
			else
			{
				Logger.Error("Failed to connect! Error: {@Message} ({@Code})", msg.Message, msg.Code);

				ClientReject();
			}
		}

		#endregion

		#region Messages

		public struct JoinRequestMessage : NetworkMessage
		{
			public string ApplicationVersion;
		}

		public struct JoinRequestResponseMessage : NetworkMessage
		{
			public int Code;
			public string Message;
		}

		#endregion
	}
}