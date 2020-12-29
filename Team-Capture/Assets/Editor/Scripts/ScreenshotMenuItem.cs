using System;
using System.IO;
using Team_Capture.Core;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor
{
    public static class ScreenshotMenuItem
    {
	    private static string screenshotDir = $"{Game.GetGameExecutePath()}/Screenshots/";

		[MenuItem("Screenshot/Take Screenshot")]
	    public static void TakeScreenshot()
	    {
		    if (!Directory.Exists(screenshotDir))
			    Directory.CreateDirectory(screenshotDir);

		    string fileName = $"{screenshotDir}{DateTime.Now:yy-MM-dd-hh-mm-ss}.png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log($"Saved screenshot to {fileName}");
	    }
    }
}