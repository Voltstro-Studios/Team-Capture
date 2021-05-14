using Cysharp.Threading.Tasks;
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
				SendRequestResponseMessage(conn, HttpCode.Ok, "Ok");
				ServerAccept(conn);
				Logger.Debug("Accepted client {Id}", conn.connectionId);
			}
			else
			{
				SendRequestResponseMessage(conn, HttpCode.PreconditionFailed, "Server and client versions mismatch!");
				Logger.Warn("Client {Id} had mismatched versions with the server! Rejecting connection.", conn.connectionId);

				conn.isAuthenticated = false;
				DisconnectClientDelayed(conn).Forget();
			}
		}

		private async UniTask DisconnectClientDelayed(NetworkConnection conn)
		{
			await Integrations.UniTask.UniTask.Delay(1000);

			ServerReject(conn);
		}

		private void SendRequestResponseMessage(NetworkConnection conn, HttpCode code, string message)
		{
			conn.Send(new JoinRequestResponseMessage
			{
				Code = code,
				Message = message
			});
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
			if (msg.Code == HttpCode.Ok)
			{
				Logger.Info("Join request was accepted! {Message} ({Code})", msg.Message, (int)msg.Code);

				ClientAccept();
			}
			else
			{
				Logger.Error("Failed to connect! Error: {Message} ({Code})", msg.Message, (int)msg.Code);

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
			public HttpCode Code;
			public string Message;
		}

		#endregion
	}
}