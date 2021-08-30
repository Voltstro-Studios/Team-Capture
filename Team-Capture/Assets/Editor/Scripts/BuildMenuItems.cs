// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Diagnostics;
using System.IO;
using UnityEditor;
using Voltstro.UnityBuilder.Build;
using Debug = UnityEngine.Debug;

namespace Team_Capture.Editor
{
	public static class BuildMenuItems
	{
		[MenuItem("Team Capture/Build/Launch Player Server")]
		public static void LaunchPlayerServer()
		{
			//Make sure the build exists
			if (!BuildExist())
			{
				Debug.LogError("No build exists! Build one using `Tools -> Volt Unity Builder -> Volt Builder -> Build Player`");
				return;
			}

			//Make sure the run server script still exists
			string buildDir = $"{GameBuilder.GetBuildDirectory()}Team-Capture-Quick/";
			if (!File.Exists($"{buildDir}RunServer.ps1"))
			{
				Debug.LogError("The build is missing the 'RunServer.ps1' script!");
				return;
			}

			Process.Start(new ProcessStartInfo
			{
				FileName = "pwsh",
				Arguments = $"{buildDir}RunServer.ps1",
				WorkingDirectory = buildDir
			});
		}

		[MenuItem("Team Capture/Build/Launch Player Client")]
		public static void LaunchPlayerClient()
		{
			//Make sure the build exists
			if (!BuildExist())
			{
				Debug.LogError("No build exists! Build one using `Tools -> Volt Unity Builder -> Volt Builder -> Build Player`");
				return;
			}

			//Make sure the run client script still exists
			string buildDir = $"{GameBuilder.GetBuildDirectory()}Team-Capture-Quick/";
			
#if UNITY_EDITOR_WIN
			string appName = "Team-Capture.exe";
#else
			string appName = "Team-Capture";
#endif
			
			if (!File.Exists($"{buildDir}{appName}"))
			{
				Debug.LogError("There is no build!");
				return;
			}

			Process.Start(new ProcessStartInfo
			{
				FileName = appName,
				Arguments = "-novid -high",
				WorkingDirectory = buildDir
			});
		}

		private static bool BuildExist()
		{
			string buildDir = $"{GameBuilder.GetBuildDirectory()}Team-Capture-Quick/";
			if (!Directory.Exists(buildDir))
				return false;

#if UNITY_EDITOR_WIN
			string applicationName = "Team-Capture.exe";
#else
			string applicationName = "Team-Capture";
#endif

			return File.Exists($"{buildDir}{applicationName}");
		}
	}
}