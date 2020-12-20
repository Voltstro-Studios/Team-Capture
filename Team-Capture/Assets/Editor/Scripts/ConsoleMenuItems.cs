using System.Collections.Generic;
using System.Reflection;
using Team_Capture.Console;
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

			MarkdownTableGenerator generator = new MarkdownTableGenerator("Command List", "Command", "Summary", "Run Permission	", "Graphics Only");
			foreach (ConCommand command in commands.Keys)
				generator.AddOption($"`{command.Name}`", command.Summary, command.RunPermission.ToString(), command.GraphicsModeOnly ? "✔" : "❌");

			generator.SaveMarkdown("command-list");
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

			MarkdownTableGenerator generator = new MarkdownTableGenerator("ConVar List", "Command", "Summary", "Graphics Only");
			foreach (ConVar command in conVars.Keys)
				generator.AddOption($"`{command.Name}`", command.Summary, command.GraphicsOnly ? "✔" : "❌");

			generator.SaveMarkdown("convar-list");
		}
	}
}