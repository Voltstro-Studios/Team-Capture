// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.AddressablesAddons;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console.Fun
{
    /// <summary>
    ///     Splash messages for the console
    /// </summary>
    internal static class SplashMessagesManager
    {
        private const string SplashMessagesPath = "Assets/Settings/SplashMessages.asset";

        private static readonly CachedFile<SplashMessages> SplashMessage =
            new(SplashMessagesPath);

        [ConCommand("splashmessage", "Shows a random splash message")]
        public static void SplashMessageCommand(string[] args)
        {
            SplashMessages settings = SplashMessage.Value;
            if (settings == null)
                return;

            //Select random splash message
            int index = Random.Range(0, settings.messages.Length);
            Logger.Info($"	{settings.messages[index]}");
        }
    }
}