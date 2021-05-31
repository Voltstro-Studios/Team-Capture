// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using Team_Capture.Core;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Team_Capture.Editor
{
    public static class ScreenshotMenuItems
    {
	    private static readonly string ScreenshotDir = $"{Game.GetGameExecutePath()}/Screenshots/";

		[MenuItem("Screenshot/Take Screenshot")]
	    public static void TakeScreenshot()
	    {
		    CheckScreenshotDirectory();

		    string fileName = $"{ScreenshotDir}{DateTime.Now:yy-MM-dd-hh-mm-ss}.png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log($"Saved screenshot to {fileName}");
	    }

	    [MenuItem("Screenshot/Open Screenshot Directory")]
	    public static void OpenScreenshotDirectory()
	    {
		    CheckScreenshotDirectory();

		    Process.Start(ScreenshotDir);
	    }

	    private static void CheckScreenshotDirectory()
	    {
		    if (!Directory.Exists(ScreenshotDir))
			    Directory.CreateDirectory(ScreenshotDir);
	    }
    }
}