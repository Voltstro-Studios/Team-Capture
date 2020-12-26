using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Voltstro.CommandLineParser;

namespace Team_Capture.Editor
{
	public static class ArgumentsMenuItems
	{
		[MenuItem("Team Capture/Arguments/Launch Arguments to Markdown")]
		public static void LaunchArgumentsToMarkdown()
		{
			Dictionary<FieldInfo, CommandLineArgumentAttribute> launchArguments = CommandLineParser.GetCommandFields();

			if (launchArguments.Count == 0)
			{
				Debug.LogError("There are no launch arguments!");
				return;
			}

			MarkdownTableGenerator generator = new MarkdownTableGenerator("Launch Arguments List", "Argument");
			foreach (KeyValuePair<FieldInfo, CommandLineArgumentAttribute> argument in launchArguments)
				generator.AddOption($"`{argument.Value.Name}`");

			generator.SaveMarkdown("launch-arguments");
		}
	}
}