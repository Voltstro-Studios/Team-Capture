// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
namespace Team_Capture.Settings.SettingClasses
{
    internal class LinuxSettingsClass : Setting
    {
        public LinuxTerminalSettings linuxTerminalSettings = new LinuxTerminalSettings
        {
            TerminalCommand = "x-terminal-emulator",
            TerminalExecute = "-e '{0} {1}'"
        };
    }

    internal struct LinuxTerminalSettings
    {
        public string TerminalCommand;
        public string TerminalExecute;
    }
}
#endif