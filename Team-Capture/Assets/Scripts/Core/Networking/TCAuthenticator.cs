using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.UserAccount;
using UnityCommandLineParser;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
	internal class TCAuthenticator : NetworkAuthenticator
	{
		[ConVar("sv_auth_method", "What account system to use to check clients")]
		[CommandLineArgument("auth-method", "What account system to use to check clients")]
		public static AccountProvider ServerAuthMethod = AccountProvider.Steam;

		[ConVar("sv_auth_clean_names", "Will trim whitespace at the start and end of account names")]
		public static bool CleanAccountNames = true;
		
		#region Server

		private Dictionary<int, Account> authAccounts;

		public Account GetAccount(int id)
		{
			Account account = authAccounts[id];
			if (account == null)
				throw new ArgumentException();

			return account;
		}

		public void ClientDisconnect(int id)
		{
			authAccounts.Remove(id);
		}

		public override void OnStartServer()
		{
			//TODO: Support auth with other service providers
			ServerAuthMethod = AccountProvider.Offline;

			authAccounts = new Dictionary<int, Account>();
			
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
			//Check versions
			if (msg.ApplicationVersion != Application.version)
			{
				SendRequestResponseMessage(conn, HttpCode.PreconditionFailed, "Server and client versions mismatch!");
				Logger.Warn("Client {Id} had mismatched versions with the server! Rejecting connection.", conn.connectionId);

				RefuseClientConnection(conn);
				return;
			}

			//Make sure they at least provided an account
			if (msg.NetworkedAccounts.Length == 0)
			{
				SendRequestResponseMessage(conn, HttpCode.Unauthorized, "No accounts provided!");
				Logger.Warn("Client {Id} sent no user accounts. Rejecting connection.", conn.connectionId);

				RefuseClientConnection(conn);
				return;
			}

			//Account auth
			if (ServerAuthMethod != AccountProvider.Offline)
			{
			}
			//We are running in Offline
			else
			{
				NetworkedAccount offlineAccount =
					msg.NetworkedAccounts.FirstOrDefault(x => x.AccountProvider == AccountProvider.Offline);

				if (string.IsNullOrWhiteSpace(offlineAccount.AccountName.String))
				{
					SendRequestResponseMessage(conn, HttpCode.Unauthorized, "Username cannot be empty or white space!");
					Logger.Warn("Client {Id} sent a blank username. Rejecting connection.", conn.connectionId);
					
					RefuseClientConnection(conn);
					return;
				}

				//Make sure there are no duplicate names already on the server, otherwise add the number to the end
				string accountName = offlineAccount.AccountName.String;
				if (CleanAccountNames)
					accountName = accountName.TrimStart().TrimEnd();
				
				int duplicates = authAccounts.Count(x => x.Value.AccountName == accountName);
				if (duplicates != 0)
					accountName += $" ({duplicates})";

				authAccounts.Add(conn.connectionId, new Account
				{
					AccountProvider = AccountProvider.Offline,
					AccountName = accountName
				});
			}

			SendRequestResponseMessage(conn, HttpCode.Ok, "Ok");
			ServerAccept(conn);
			Logger.Debug("Accepted client {Id}", conn.connectionId);
		}

		private void RefuseClientConnection(NetworkConnection conn)
		{
			conn.isAuthenticated = false;
			DisconnectClientDelayed(conn).Forget();
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
				ApplicationVersion = Application.version,
				NetworkedAccounts = User.GetAccountsAsNetworked()
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

		private struct JoinRequestMessage : NetworkMessage
		{
			public string ApplicationVersion;

			internal NetworkedAccount[] NetworkedAccounts;
		}

		private struct JoinRequestResponseMessage : NetworkMessage
		{
			public HttpCode Code;
			public string Message;
		}

		#endregion
	}
}