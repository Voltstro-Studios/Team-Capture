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
	    private static string screenshotDir = $"{Game.GetGameExecutePath()}/Screenshots/";

		[MenuItem("Screenshot/Take Screenshot")]
	    public static void TakeScreenshot()
	    {
		    CheckScreenshotDirectory();

		    string fileName = $"{screenshotDir}{DateTime.Now:yy-MM-dd-hh-mm-ss}.png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log($"Saved screenshot to {fileName}");
	    }

	    [MenuItem("Screenshot/Open Screenshot Directory")]
	    public static void OpenScreenshotDirectory()
	    {
		    CheckScreenshotDirectory();

		    Process.Start(screenshotDir);
	    }

	    private static void CheckScreenshotDirectory()
	    {
		    if (!Directory.Exists(screenshotDir))
			    Directory.CreateDirectory(screenshotDir);
	    }
    }
}