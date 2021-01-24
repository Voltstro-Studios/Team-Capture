using System;
using System.Collections.Generic;
using UnityEditor;
using Voltstro.UnityBuilder.Build;

namespace Team_Capture.Editor
{
    public static class CIBuilder
    {
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