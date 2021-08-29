// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Steamworks;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Integrations.Steamworks
{
    public static class SteamServerManager
    {
        private static Dictionary<SteamUser, AuthResult> authResults;

        public static void StartServer()
        {
            SteamServerInit serverInit = new SteamServerInit("Team-Capture", "Team-Capture")
            {
                DedicatedServer = true,
                Secure = true,
                VersionString = Application.version,
                IpAddress = IPAddress.Any,
                GamePort = 7777
            };
            
            authResults = new Dictionary<SteamUser, AuthResult>();

            SteamServer.OnSteamServersConnected += () => Logger.Info("Server has connected to Steam game servers.");
            SteamServer.OnSteamServerConnectFailure += (result, retry) =>
                { Logger.Error("Server failed to connect to Steam game servers! Reason: {Reason} Retry: {Retry}", result, retry); };
            SteamServer.OnValidateAuthTicketResponse += OnAuthResponse;

            try
            {
                SteamServer.Init(SteamSettings.SteamSettingsInstance.appDedicatedServerId, serverInit);
                SteamServer.LogOnAnonymous();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Something went wrong while starting the steam game server integration!");
            }
        }

        private static void OnAuthResponse(SteamId steamId, SteamId ownerId, AuthResponse status)
        {
            //TODO: If the user cancels their auth ticket, we need to disconnect them
            KeyValuePair<SteamUser, AuthResult> user = authResults.FirstOrDefault(x => x.Key.UserId == steamId);
            if (user.Equals(null))
                return;
            
            Logger.Info("Got client {ID} auth response back of: {status}", steamId, status);

            if (status == AuthResponse.OK)
                user.Value.OnSuccess.Invoke();
            else
                user.Value.OnFail.Invoke();

            authResults.Remove(user.Key);
        }

        public static void ShutdownServer()
        {
            Logger.Info("Shutting down connection for Steam game server...");
            SteamServer.LogOff();
            SteamServer.Shutdown();
        }

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

        public static void RunCallbacks()
        {
            SteamServer.RunCallbacks();
        }

        private class AuthResult
        {
            public Action OnSuccess;
            public Action OnFail;
        }
    }
}