// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voltstro.UnityBuilder.Actions;

namespace Team_Capture.Editor
{
    /// <summary>
    ///     Custom TC <see cref="IBuildAction"/> for Volt build
    /// </summary>
    public class TVVoltAction : IBuildAction
    {
        public void OnGUI()
        {
        }

        public void OnBeforeBuild(string buildLocation, BuildTarget buildTarget, ref BuildOptions buildOptions)
        {
            if (buildTarget == BuildTarget.StandaloneWindows64)
            {
                buildOptions |= BuildOptions.EnableHeadlessMode;
            }
        }

        public void OnAfterBuild(string buildLocation, BuildReport report)
        {
            if (report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                string location = Path.GetFullPath($"{buildLocation}/Team-Capture_Data/boot.config");
                if (!File.Exists(location))
                    throw new FileNotFoundException(
                        $"The boot config file couldn't be found at '{location}'!");
                
                Debug.Log($"Altering '{location}'");
                string[] lines = File.ReadAllLines(location);
                lines = lines.Take(lines.Length - 2).ToArray();
                File.WriteAllLines(location, lines);
            }
        }
    }
}
