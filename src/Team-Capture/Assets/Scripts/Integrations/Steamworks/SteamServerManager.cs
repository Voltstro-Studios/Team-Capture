// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using NetFabric.Hyperlinq;
using Steamworks;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Integrations.Steamworks
{
    /// <summary>
    ///     Integration with Steam game server
    /// </summary>
    public static class SteamServerManager
    {
        private static Dictionary<SteamUser, AuthResult> authResults;

        internal static bool IsOnline { get; private set; }

        /// <summary>
        ///     Start Steam game server
        /// </summary>
        /// <param name="onFail"></param>
        internal static void StartServer(Action onFail)
        {
            SteamServerInit serverInit = new("Team-Capture", "Team-Capture")
            {
                DedicatedServer = true,
                Secure = true,
                VersionString = Application.version,
                IpAddress = IPAddress.Any,
                GamePort = 7777
            };

            authResults = new Dictionary<SteamUser, AuthResult>();

            SteamServer.OnSteamServersConnected += () => Logger.Info("Server has connected to Steam game servers.");
            SteamServer.OnSteamServerConnectFailure += (result, retry) => OnSteamConnectFail(result, onFail);
            SteamServer.OnValidateAuthTicketResponse += OnAuthResponse;

            try
            {
                SteamServer.Init(SteamManager.Instance.SteamSettings.appDedicatedServerId, serverInit);
                SteamServer.LogOnAnonymous();
                IsOnline = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Something went wrong while starting the steam game server integration!");
                IsOnline = false;
                onFail.Invoke();
            }
        }

        private static void OnSteamConnectFail(Result result, Action onFail)
        {
            Logger.Error("Failed to connect to Steam game servers! Result: {Result}", result);
            SteamServer.LogOff();

            //ShutdownServer();
            onFail.Invoke();
        }

        private static void OnAuthResponse(SteamId steamId, SteamId ownerId, AuthResponse status)
        {
            //TODO: If the user cancels their auth ticket, we need to disconnect them
            Option<KeyValuePair<SteamUser, AuthResult>> result =
                authResults.AsValueEnumerable().Where(x => x.Key.UserId == steamId).First();
            if (result.IsNone)
                return;

            (SteamUser key, AuthResult authResult) = result.Value;
            if (key.Equals(null))
                return;

            authResults.Remove(key);

            Logger.Info("Got client {ID} auth response back of: {status}", steamId, status);

            if (status == AuthResponse.OK)
                authResult.OnSuccess.Invoke();
            else
                authResult.OnFail.Invoke();
        }

        /// <summary>
        ///     Shutdown Steam game server
        /// </summary>
        internal static void ShutdownServer()
        {
            if (!IsOnline)
                return;

            Logger.Info("Shutting down connection for Steam game server...");
            SteamServer.LogOff();
            SteamServer.Shutdown();
            IsOnline = false;
        }

        /// <summary>
        ///     Begins <see cref="SteamServer.BeginAuthSession" /> on the <see cref="SteamUser" />
        /// </summary>
        /// <param name="user">The user to auth</param>
        /// <param name="onSuccess">Invoked if auth is a success</param>
        /// <param name="onFail">Invoked if auth was a fail</param>
        public static void BeginAuthUser(SteamUser user, Action onSuccess, Action onFail)
        {
            Logger.Info("Begin client {ID} auth session...", user.UserId);
            authResults.Add(user, new AuthResult
            {
                OnSuccess = onSuccess,
                OnFail = onFail
            });
            SteamServer.BeginAuthSession(user.AuthTicket.Data, user.UserId);
        }

        /// <summary>
        ///     Cancels authorization of a user
        /// </summary>
        /// <param name="user"></param>
        public static void CancelAuthUser(SteamUser user)
        {
            if (authResults.ContainsKey(user))
                authResults.Remove(user);
        }

        /// <summary>
        ///     Runs Steam game server callbacks
        /// </summary>
        internal static void RunCallbacks()
        {
            if (!IsOnline)
                return;

            SteamServer.RunCallbacks();
        }

        private class AuthResult
        {
            public Action OnFail;
            public Action OnSuccess;
        }
    }
}