// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.AddressablesAddons;
using Team_Capture.Logging;

namespace Team_Capture.Console.Fun
{
    /// <summary>
    ///     Manager for the splash screen
    /// </summary>
    internal static class SplashScreenManager
    {
        private const string SplashScreenPath = "Assets/Settings/SplashScreen.asset";

        private static readonly CachedFile<SplashScreen> SplashScreenCache =
            new(SplashScreenPath);

        [ConCommand("asciiart", "Shows Team-Capture ascii art")]
        public static void AsciiArtCommand(string[] args)
        {
            SplashScreen splashScreen = SplashScreenCache.Value;
            if (splashScreen == null)
                return;

            Logger.Info(splashScreen.splashScreen);
        }
    }
}