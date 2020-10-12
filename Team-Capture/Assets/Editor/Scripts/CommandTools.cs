using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Console;
using UnityEditor;
using UnityEngine;

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

			StringBuilder sb = new StringBuilder();
			sb.Append("## Command List\n\n");
			sb.Append("|Command|Summary|Run Permission|\n");
			sb.Append("|-------|-------|--------------|\n");
			foreach (ConCommand command in commands.Keys)
			{
				sb.Append($"|`{command.Name}`|{command.Summary}|{command.RunPermission}|\n");
			}

			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "command-list", "md");
			if(path.Length == 0)
				return;

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

			StringBuilder sb = new StringBuilder();
			sb.Append("## ConVar List\n\n");
			sb.Append("|Command|Summary|\n");
			sb.Append("|-------|-------|\n");
			foreach (ConVar command in conVars.Keys)
			{
				sb.Append($"|`{command.Name}`|{command.Summary}|\n");
			}

			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "convar-list", "md");
			if(path.Length == 0)
				return;

			File.WriteAllText(path, sb.ToString());
			Debug.Log($"Saved list to `{path}`.");
		}
	}
}