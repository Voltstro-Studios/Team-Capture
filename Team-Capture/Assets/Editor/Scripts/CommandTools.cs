using System.Collections.Generic;
using System.IO;
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
			if (EditorApplication.isPlaying)
			{
				Dictionary<string, ConsoleCommand> commands = ConsoleBackend.GetAllCommands();
				if (commands == null)
				{
					Debug.LogError("Console system doesn't have any commands! Either its not active or something went wrong.");
					return;
				}

				StringBuilder sb = new StringBuilder();
				sb.Append("# Command List\n\n");
				sb.Append("|Command|Summary|Run Permission|\n");
				sb.Append("|-------|-------|--------------|\n");
				foreach (KeyValuePair<string, ConsoleCommand> command in commands)
				{
					sb.Append($"|`{command.Key}`|{command.Value.CommandSummary}|{command.Value.RunPermission}|\n");
				}

				string path = EditorUtility.SaveFilePanel("Save markdown file", "", "command-list", ".md");
				if(path.Length == 0)
					return;

				File.WriteAllText(path, sb.ToString());
				return;
			}

			Debug.LogError("The game needs to be running inorder to use this!");
		}
	}
}