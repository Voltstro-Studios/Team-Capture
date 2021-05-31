// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityCommandLineParser;

namespace Team_Capture.Editor
{
	public static class ArgumentsMenuItems
	{
		[MenuItem("Team Capture/Arguments/Launch Arguments to Markdown")]
		public static void LaunchArgumentsToMarkdown()
		{
			Dictionary<FieldInfo, CommandLineArgumentAttribute> launchArguments = CommandLineParser.GetCommandLineArguments();

			if (launchArguments.Count == 0)
			{
				Debug.LogError("There are no launch arguments!");
				return;
			}

			MarkdownTableGenerator generator = new MarkdownTableGenerator("Launch Arguments List", "Argument", "Description");
			foreach (KeyValuePair<FieldInfo, CommandLineArgumentAttribute> argument in launchArguments)
				generator.AddOption($"`{argument.Value.Name}`", argument.Value.Description);

			generator.SaveMarkdown("launch-arguments");
		}
	}
}