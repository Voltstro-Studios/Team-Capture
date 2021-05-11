using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Voltstro.UnityBuilder.Build;
using Voltstro.UnityBuilder.Settings;

namespace Team_Capture.Editor
{
	/// <summary>
	///		A class with the intent to be used by Unity editor headless mode.
	///		<para>
	///			This is so a CI integration can run Unity and tell it to run the <see cref="StartVoltBuilder"/> method.
	///			You NEED to add <c>-buildTarget [target]</c> as a launch option when running the editor so Volt builder can know what platform to build for.
	///		</para>
	/// </summary>
    public static class CIBuilder
	{
		private const string ZipBuildKey = "ZipBuild";
	    
		/// <summary>
		///		Builds the game using Volt Builder's <see cref="GameBuilder.BuildGame"/>
		/// </summary>
        public static void StartVoltBuilder()
        {
	        Debug.Log("Build game started...");
			System.Console.WriteLine("Build game started...");

			ParseCommandLineArguments(out Dictionary<string, string> arguments);

			if(!arguments.ContainsKey("buildTarget"))
				EditorApplication.Exit(-1);

			bool currentZipBuild = false;
			if (SettingsManager.Instance.ContainsKey<bool>(ZipBuildKey))
				currentZipBuild = SettingsManager.Instance.Get<bool>(ZipBuildKey);
			SettingsManager.Instance.Set(ZipBuildKey, true);
			
	        BuildTarget target = (BuildTarget) Enum.Parse(typeof(BuildTarget), arguments["buildTarget"]);
			string buildDir = $"{GameBuilder.GetBuildDirectory()}{target}-DevOpsBuild/{PlayerSettings.productName}";

			System.Console.WriteLine($"Building TC for {target} platform to {buildDir}");
			
			if(target == BuildTarget.StandaloneWindows64)
				GameBuilder.BuildGame(buildDir, target, true);
			else
				GameBuilder.BuildGame(buildDir, target, false);

	        SettingsManager.Instance.Set(ZipBuildKey, currentZipBuild);
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
	        providedArguments = new Dictionary<string, string>();
	        string[] args = Environment.GetCommandLineArgs();

	        // Extract flags with optional values
	        for (int current = 0, next = 1; current < args.Length; current++, next++) 
	        {
		        // Parse flag
		        bool isFlag = args[current].StartsWith("-");
		        if (!isFlag) continue;

		        string flag = args[current].TrimStart('-');

		        // Parse optional value
		        bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
		        string value = flagHasValue ? args[next].TrimStart('-') : "";
		        providedArguments.Add(flag, value);
	        }
        }
    }
}