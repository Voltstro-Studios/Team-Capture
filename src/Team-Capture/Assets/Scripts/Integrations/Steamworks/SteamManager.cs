// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Steamworks;
using Team_Capture.Core.Networking;
using Team_Capture.Logging;
using Team_Capture.UserManagement;

namespace Team_Capture.Integrations.Steamworks
{
    /// <summary>
    ///     Handles connecting to Steam
    /// </summary>
    [CreateOnInit]
    internal partial class SteamManager : SingletonMonoBehaviourSettings<SteamManager, SteamSettings>
    {
        protected override string SettingsPath => "Assets/Settings/Integrations/SteamSettings.asset";

        internal SteamSettings SteamSettings => Settings;

        private void Update()
        {
            SteamClient.RunCallbacks();
        }

        protected override void SingletonStarted()
        {
            //TODO: We should have a global 'offline' toggle
            if (TCAuthenticator.AuthMethod != UserProvider.Steam)
            {
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        protected override void SingletonDestroyed()
        {
            SteamClient.Shutdown();
        }

        private void Initialize()
        {
            try
            {
                SteamClient.Init(Settings.appId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Something went wrong while starting the Steam integration!");
                Destroy(gameObject);
                return;
            }

            if (!SteamClient.IsLoggedOn)
            {
                Logger.Error("Could not connect to Steam!");
                Destroy(gameObject);
                return;
            }

            User.AddUser(new SteamUser(SteamClient.SteamId));

            Logger.Info("Logged into Steam account {AccountName} with an ID of {AccountID}", SteamClient.Name,
                SteamClient.SteamId.Value);
        }
    }
}