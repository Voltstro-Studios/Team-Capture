// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Diagnostics;
using UnityCommandLineParser;
using UnityEngine;

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
            if(setToHighPriority)
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }
    }
}
