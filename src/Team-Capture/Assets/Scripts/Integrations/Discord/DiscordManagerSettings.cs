// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Discord.GameSDK;
using Team_Capture.AddressablesAddons;
using UnityEngine;

namespace Team_Capture.Integrations.Discord
{
    [CreateAssetMenu(fileName = "Discord Settings", menuName = "Team-Capture/Settings/Integrations/Discord Settings")]
    internal class DiscordManagerSettings : ScriptableObject
    {
        /// <summary>
        ///     The client ID that we will use
        /// </summary>
        [Tooltip("The client ID that we will use")]
        public string clientId;

        /// <summary>
        ///     Text used when loading
        /// </summary>
        public CachedLocalizedString loadingText;

        /// <summary>
        ///     Text used to show when playing
        /// </summary>
        public CachedLocalizedString playingOnText;

        /// <summary>
        ///     Main menu text
        /// </summary>
        public CachedLocalizedString mainMenuText;

        /// <summary>
        ///     The log level to use
        /// </summary>
        [Tooltip("The log level to use")] public LogLevel logLevel = LogLevel.Warn;
    }
}