﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Text;
using Discord.GameSDK;
using Discord.GameSDK.Activities;
using Discord.GameSDK.Users;
using Team_Capture.Core;
using Team_Capture.Helper;
using Team_Capture.SceneManagement;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Integrations.Discord
{
    /// <summary>
    ///     Handles communicating with Discord's game SDK
    /// </summary>
    [CreateOnInit]
    internal partial class DiscordManager : SingletonMonoBehaviourSettings<DiscordManager, DiscordManagerSettings>
    {
        private ActivityManager activityManager;

        private global::Discord.GameSDK.Discord client;
        private UserManager userManager;
        protected override string SettingsPath => "Assets/Settings/Integrations/DiscordSettings.asset";

        private void Update()
        {
            client?.RunCallbacks();
        }

        protected override void SingletonStarted()
        {
            if (Game.IsHeadless)
            {
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        protected override void SingletonDestroyed()
        {
            //Using Null Propagation seems to crash Unity...
            // ReSharper disable once UseNullPropagation
            if (client != null)
                client.Dispose();
        }

        private void Initialize()
        {
            if (client != null)
            {
                Logger.Error("The discord client is already running!");
                return;
            }

            try
            {
                client = new global::Discord.GameSDK.Discord(long.Parse(Settings.clientId),
                    CreateFlags.NoRequireDiscord);
                client.Init();
            }
            catch (ResultException ex)
            {
                Logger.Error("Failed to connect with Discord! {@Message} {@ResultCode}", ex.Message, ex.Result);
                client = null;
                Destroy(gameObject);
                return;
            }

            client?.SetLogHook(Settings.logLevel, (level, message) =>
            {
                switch (level)
                {
                    case LogLevel.Error:
                        Logger.Error(message);
                        break;
                    case LogLevel.Warn:
                        Logger.Warn(message);
                        break;
                    case LogLevel.Info:
                        Logger.Info(message);
                        break;
                    case LogLevel.Debug:
                        Logger.Debug(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            });
            activityManager = client?.GetActivityManager();
            userManager = client?.GetUserManager();
            
            TCScenesManager.OnSceneLoadedEvent += SceneLoaded;

            SceneLoaded(TCScenesManager.GetActiveScene());
        }

        /// <summary>
        ///     Updates the active Discord activity that is shown (AkA the Rich Presence)
        /// </summary>
        /// <param name="activity"></param>
        public static void UpdateActivity(Activity activity)
        {
            if (Instance == null) return;
            if (Instance.client == null) return;

            Instance.activityManager.UpdateActivity(activity,
                result => { Logger.Debug($"[Discord Presence] Updated activity: {result}"); });
        }

        #region Scene Discord RPC Stuff

        private void SceneLoaded(TCScene scene)
        {
            if (client != null)
            {
                Activity presence = new()
                {
                    Assets = new ActivityAssets
                    {
                        LargeImage = scene.largeImageKey
                    }
                };

                if (scene.showStartTime)
                    presence.Timestamps = new ActivityTimestamps
                    {
                        Start = TimeHelper.UnixTimeNow()
                    };

                if (scene.isOnlineScene)
                {
                    presence.Details = ZString.Format(Settings.playingOnText, scene.DisplayNameLocalized);
                    presence.Assets.LargeText = scene.largeImageKeyText;
                }
                else if (scene.isMainMenu)
                {
                    presence.Details = Settings.mainMenuText;
                }
                else if (!scene.isMainMenu && !scene.isOnlineScene)
                {
                    presence.Details = Settings.loadingText;
                }
                else
                {
                    Logger.Error("You CANNOT have an online scene and a main menu scene!");
                }

                UpdateActivity(presence);
            }
        }

        #endregion
    }
}