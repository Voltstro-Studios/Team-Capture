using System;
using System.Collections.Generic;
using System.IO;
using Unity.SharpZipLib.Utils;
using UnityEditor;
using Voltstro.UnityBuilder.Build;

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
		/// <summary>
		///		Builds the game using Volt Builder's <see cref="GameBuilder.BuildGame"/>
		/// </summary>
        public static void StartVoltBuilder()
        {
			System.Console.WriteLine("Build game started...");

			ParseCommandLineArguments(out Dictionary<string, string> arguments);

			if(!arguments.ContainsKey("buildTarget"))
				EditorApplication.Exit(-1);

			BuildTarget target = (BuildTarget) Enum.Parse(typeof(BuildTarget), arguments["buildTarget"]);
			string buildDir = $"{GameBuilder.GetBuildDirectory()}{target}-DevOpsBuild/{PlayerSettings.productName}";

			System.Console.WriteLine($"Building TC for {target} platform to {buildDir}");

	        GameBuilder.BuildGame(buildDir, target);
	        
	        //Zip
	        string outPath = $"{buildDir}/../{Path.GetFileName(buildDir)}.zip";
	        System.Console.WriteLine($"Zipping to {outPath}");
	        ZipUtility.CompressFolderToZip(outPath, null, Path.GetFullPath(buildDir));
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