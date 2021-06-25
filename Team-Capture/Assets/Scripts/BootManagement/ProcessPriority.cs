// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Diagnostics;
using UnityCommandLineParser;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.BootManagement
{
    [CreateAssetMenu(fileName = "Process Priority", menuName = "BootManager/Process Priority")]
    internal class ProcessPriority : BootItem
    {
        private static bool setToHighPriority;

        [CommandLineCommand("high", "Set the process to high priority")]
        public static void HighPriorityProcess()
        {
            setToHighPriority = true;
        }
        
        public override void OnBoot()
        {
            try
            {
                if (setToHighPriority)
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to set current process to high priority!");
            }
        }
    }
}
