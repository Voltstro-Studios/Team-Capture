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
		[MenuItem("Team Capture/Console/Command list to markdown")]
		public static void ConsoleCommandListToMarkdown()
		{
			Dictionary<ConCommand, MethodInfo> commands = ConsoleBackend.GetConCommands();
			if (commands == null)
			{
				Debug.LogError("Console system doesn't have any commands! Either its not active or something went wrong.");
				return;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("# Command List\n\n");
			sb.Append("|Command|Summary|Run Permission|\n");
			sb.Append("|-------|-------|--------------|\n");
			foreach (ConCommand command in commands.Keys)
			{
				sb.Append($"|`{command.Name}`|{command.Summary}|{command.RunPermission}|\n");
			}

			string path = EditorUtility.SaveFilePanel("Save markdown file", "", "command-list", ".md");
			if(path.Length == 0)
				return;

			File.WriteAllText(path, sb.ToString());
		}
	}
}