// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Diagnostics;
using UnityCommandLineParser;
using UnityEngine;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.BootManagement
{
    [Preserve]
    internal static class ProcessPriority
    {
        private static bool setToHighPriority;

        [CommandLineCommand("high", "Set the process to high priority")]
        public static void HighPriorityProcess()
        {
            setToHighPriority = true;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            try
            {
                if (setToHighPriority)
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                    Logger.Debug("Process was set to high priority.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to set current process to high priority!");
            }
        }
    }
}
