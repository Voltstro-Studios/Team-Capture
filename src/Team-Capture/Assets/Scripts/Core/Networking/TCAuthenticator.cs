// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Mirror;
using NetFabric.Hyperlinq;
using Team_Capture.Console;
using Team_Capture.Integrations.Steamworks;
using Team_Capture.UserManagement;
using UnityCommandLineParser;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.Networking
{
    /// <summary>
    ///     <see cref="NetworkAuthenticator" /> for Team-Capture
    /// </summary>
    internal class TCAuthenticator : NetworkAuthenticator
    {
        [ConVar("sv_auth_method", "What account system to use to check clients")]
        [CommandLineArgument("auth-method", "What account system to use to check clients")]
        public static UserProvider AuthMethod = UserProvider.Steam;

        #region Server

        private Dictionary<int, IUser> inProgressAuth;
        private Dictionary<int, IUser> authAccounts;

        /// <summary>
        ///     Gets an account from their connection ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CanBeNull]
        public IUser GetAccount(int id)
        {
            if (!authAccounts.ContainsKey(id))
                return null;

            IUser account = authAccounts[id];
            return account;
        }

        public override void OnStartServer()
        {
            if (AuthMethod == UserProvider.Steam)
            {
                Logger.Info("Starting Steam game server integration...");
                SteamServerManager.StartServer(() =>
                {
                    Logger.Error("Falling back to offline mode!");
                    AuthMethod = UserProvider.Offline;
                });
            }

            inProgressAuth = new Dictionary<int, IUser>();
            authAccounts = new Dictionary<int, IUser>();
            NetworkServer.RegisterHandler<JoinRequestMessage>(OnRequestJoin, false);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<JoinRequestMessage>();

            //TODO: Clean interface for other auth providers who need to start/shutdown
            SteamServerManager.ShutdownServer();
            authAccounts.Clear();
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
                Logger.Warn("Client {Id} had mismatched versions with the server! Rejecting connection.",
                    conn.connectionId);

                RefuseClientConnection(conn);
                return;
            }

            //Make sure they at least provided an account
            if (msg.UserAccounts.Length == 0)
            {
                SendRequestResponseMessage(conn, HttpCode.Unauthorized, "No accounts provided!");
                Logger.Warn("Client {Id} sent no user accounts. Rejecting connection.", conn.connectionId);

                RefuseClientConnection(conn);
                return;
            }

            Logger.Debug("Got {UserAccountsNum} user accounts from {UserId}", msg.UserAccounts.Length,
                conn.connectionId);

            //Get the user account the server wants
            Option<IUser> userResult =
                msg.UserAccounts.AsValueEnumerable().Where(x => x.UserProvider == AuthMethod).First();
            if (userResult.IsNone)
            {
                SendRequestResponseMessage(conn, HttpCode.Unauthorized, "No valid user accounts sent!");
                Logger.Warn("Client {Id} sent no valid user accounts!. Rejecting connection.", conn.connectionId);

                RefuseClientConnection(conn);
                return;
            }

            IUser user = userResult.Value;

            if (authAccounts.ContainsValue(user))
            {
                SendRequestResponseMessage(conn, HttpCode.Unauthorized, "User is already connected!");
                Logger.Warn(
                    "Client {Id} tried to connect with the same account as an existing client!. Rejecting connection.",
                    conn.connectionId);

                RefuseClientConnection(conn);
                return;
            }

            try
            {
                inProgressAuth.Add(conn.connectionId, user);
                user.ServerStartClientAuthentication(() =>
                {
                    inProgressAuth.Remove(conn.connectionId);
                    authAccounts.Add(conn.connectionId, user);

                    SendRequestResponseMessage(conn, HttpCode.Ok, "Ok");
                    ServerAccept(conn);
                    Logger.Debug("Accepted client {Id}", conn.connectionId);
                }, () =>
                {
                    SendRequestResponseMessage(conn, HttpCode.Unauthorized, "Failed authorization!");
                    Logger.Warn("Client {Id} failed to authorize!. Rejecting connection.", conn.connectionId);

                    RefuseClientConnection(conn);
                });
            }
            catch (Exception ex)
            {
                SendRequestResponseMessage(conn, HttpCode.InternalServerError,
                    "An error occured with the server authorization!");
                Logger.Error(ex, "An error occured on the server side with authorization");

                RefuseClientConnection(conn);
            }
        }

        public void OnServerClientDisconnect(NetworkConnection conn)
        {
            if (authAccounts.ContainsKey(conn.connectionId))
            {
                authAccounts.Remove(conn.connectionId);
            }

            else if (inProgressAuth.ContainsKey(conn.connectionId))
            {
                inProgressAuth[conn.connectionId].ServerCancelClientAuthentication();
                inProgressAuth.Remove(conn.connectionId);
            }
        }

        private void RefuseClientConnection(NetworkConnection conn)
        {
            conn.isAuthenticated = false;
            DisconnectClientDelayed(conn).Forget();
        }

        private async UniTask DisconnectClientDelayed(NetworkConnection conn)
        {
            await UniTask.Delay(1000);

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

        private void Update()
        {
            if (TCNetworkManager.IsServer) SteamServerManager.RunCallbacks();
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
            IUser[] users = User.GetUsers();
            foreach (IUser user in users)
                try
                {
                    user.ClientStartAuthentication();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "An error occured while trying to authenticate on the client end!");

                    ClientReject();
                    return;
                }

            Logger.Debug("Sent a total of {Num} of user accounts to the server.", users.Length);
            NetworkClient.connection.Send(new JoinRequestMessage
            {
                ApplicationVersion = Application.version,
                UserAccounts = users
            });
        }

        public void OnClientDisconnect()
        {
            foreach (IUser user in User.GetUsers()) user.ClientStopAuthentication();
        }

        private void OnReceivedJoinRequestResponse(JoinRequestResponseMessage msg)
        {
            //We good to connect
            if (msg.Code == HttpCode.Ok)
            {
                Logger.Info("Join request was accepted! {Message} ({Code})", msg.Message, (int) msg.Code);

                ClientAccept();
            }
            //Something fucked up
            else
            {
                Logger.Error("Failed to connect! Error: {Message} ({Code})", msg.Message, (int) msg.Code);

                ClientReject();
            }
        }

        #endregion

        #region Messages

        private struct JoinRequestMessage : NetworkMessage
        {
            public string ApplicationVersion;

            internal IUser[] UserAccounts;
        }

        private struct JoinRequestResponseMessage : NetworkMessage
        {
            public HttpCode Code;
            public string Message;
        }

        #endregion
    }
}