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
#if UNITY_EDITOR_WIN
		private const string ApplicationName = "Team-Capture.exe";
#else
		private const string ApplicationName = "Team-Capture";
#endif
		
		[MenuItem("Team-Capture/Build/Launch Player Server")]
		public static void LaunchPlayerServer()
		{
			LaunchApp("-nographics -batchmode");
		}
		
		[MenuItem("Team-Capture/Build/Launch Player Server (Offline)")]
		public static void LaunchPlayerServerOffline()
		{
			LaunchApp("-nographics -batchmode -auth-method Offline");
		}

		[MenuItem("Team-Capture/Build/Launch Player Client")]
		public static void LaunchPlayerClient()
		{
			LaunchApp("-novid -high");
		}
		
		[MenuItem("Team-Capture/Build/Launch Player Client (Offline)")]
		public static void LaunchPlayerClientOffline()
		{
			LaunchApp("-novid -high -auth-method Offline");
		}

		[MenuItem("Team-Capture/Build/Launch Player Server", true)]
		[MenuItem("Team-Capture/Build/Launch Player Server (Offline)", true)]
		[MenuItem("Team-Capture/Build/Launch Player Client", true)]
		[MenuItem("Team-Capture/Build/Launch Player Client (Offline)", true)]
		public static bool ValidateLaunch()
		{
			return GetBuildDir() != null;
		}

		private static void LaunchApp(string arguments)
		{
			//Make sure the build exists
			string buildPath = GetBuildDir();
			
			if (buildPath == null)
			{
				Debug.LogError("No build exists! Build one using `Tools -> Volt Unity Builder -> Volt Builder -> Build Player`");
				return;
			}
			
			string buildDirWorking = Path.GetDirectoryName(buildPath);

			Process.Start(new ProcessStartInfo
			{
				FileName = buildPath,
				Arguments = arguments,
				WorkingDirectory = buildDirWorking
			});
		}

		private static string GetBuildDir()
		{
			string buildDir = $"{GameBuilder.GetBuildDirectory()}Team-Capture-Quick/";
			if (!Directory.Exists(buildDir))
				return null;

			string fullPath = $"{buildDir}{ApplicationName}";

			return !File.Exists(fullPath) ? null : Path.GetFullPath(fullPath);
		}
	}
}