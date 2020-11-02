using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Console;
using UnityEditor;
using UnityEngine;
using Voltstro.CommandLineParser;

namespace Editor.Scripts
{
	public static class GetCommands
	{
		[MenuItem("Team Capture/Console/ConCommands list to markdown")]
		public static void ConsoleCommandListToMarkdown()
		{
			Dictionary<ConCommand, MethodInfo> commands = ConsoleBackend.GetConCommands();
			if (commands == null)
			{
				Debug.LogError("Console system doesn't have any commands! Either its not active or something went wrong.");
				return;
			}

			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "command-list", "md");
			if(path.Length == 0)
				return;

			StringBuilder sb = new StringBuilder();
			sb.Append("## Command List\n\n");
			sb.Append("|Command|Summary|Run Permission|Graphics Only|\n");
			sb.Append("|-------|-------|--------------|-------------|\n");
			foreach (ConCommand command in commands.Keys)
			{
				sb.Append($"|`{command.Name}`|{command.Summary}|{command.RunPermission}|");
				sb.Append(command.GraphicsModeOnly ? "✔|\n" : "❌|\n");
			}

			File.WriteAllText(path, sb.ToString());
			Debug.Log($"Saved list to `{path}`.");
		}

		[MenuItem("Team Capture/Console/ConVars list to markdown")]
		public static void ConsoleConVarListToMarkdown()
		{
			Dictionary<ConVar, FieldInfo> conVars = ConsoleBackend.GetConVars();
			if (conVars == null)
			{
				Debug.LogError("Console system doesn't have any commands! Either its not active or something went wrong.");
				return;
			}

			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "convar-list", "md");
			if(path.Length == 0)
				return;

			StringBuilder sb = new StringBuilder();
			sb.Append("## ConVar List\n\n");
			sb.Append("|Command|Summary|Graphics Only|\n");
			sb.Append("|-------|-------|--------------|\n");
			foreach (ConVar command in conVars.Keys)
			{
				sb.Append($"|`{command.Name}`|{command.Summary}|");
				sb.Append(command.GraphicsOnly ? "✔|\n" : "❌|\n");
			}

			File.WriteAllText(path, sb.ToString());
			Debug.Log($"Saved list to `{path}`.");
		}

		[MenuItem("Team Capture/Arguments/Launch Arguments to Markdown")]
		public static void LaunchArgumentsToMarkdown()
		{
			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "launch-arguments-list", "md");
			if(path.Length == 0)
				return;

			Dictionary<FieldInfo, CommandLineArgumentAttribute> launchArguments = CommandLineParser.GetCommandFields();

			if (launchArguments.Count == 0)
			{
				Debug.LogError("There are no launch arguments!");
				return;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("## Launch Arguments List\n\n");
			sb.Append("|Argument|\n");
			sb.Append("|--------|\n");
			foreach (KeyValuePair<FieldInfo, CommandLineArgumentAttribute> argument in launchArguments)
			{
				sb.Append($"|`{argument.Value.Name}`|\n");
			}

			File.WriteAllText(path, sb.ToString());
			Debug.Log($"Saved list to `{path}`.");
		}
	}
}