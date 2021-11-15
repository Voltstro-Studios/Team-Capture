// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Diagnostics;
using Team_Capture.Core;
using Team_Capture.Settings;
using Team_Capture.Settings.SettingClasses;

namespace Team_Capture.Helper
{
    internal static class ProcessHelper
    {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        internal static void LaunchLinuxTerminalAndLaunchProcess(string file, string arguments)
        {
            LinuxTerminalSettings terminalSettings = GameSettings.LinuxSettings.linuxTerminalSettings;
            string executeArgument = string.Format(terminalSettings.TerminalExecute, file, arguments);
            ProcessStartInfo startInfo = new()
            {
                FileName = terminalSettings.TerminalCommand,
                Arguments = executeArgument,
                WorkingDirectory = Game.GetGameExecutePath()
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
        }
#endif
    }
}