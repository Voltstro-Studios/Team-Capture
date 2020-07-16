using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Attributes;
using Delegates;
using UnityEngine;

namespace Core.Console
{
	public class ConsoleInterface : MonoBehaviour
	{
		private static readonly Dictionary<string, ConsoleCommand> Commands = new Dictionary<string, ConsoleCommand>();

		/// <summary>
		/// Finds all static methods with the <see cref="ConCommand"/> attribute attached to it
		/// and adds it to this <see cref="ConsoleInterface"/> list of commands
		/// </summary>
		public static void RegisterCommands()
		{
			const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			foreach (MethodInfo method in type.GetMethods(bindingFlags))
			{
				if (!(Attribute.GetCustomAttribute(method, typeof(ConCommand)) is ConCommand attribute))
					continue;

				MethodDelegate methodDelegate =
					(MethodDelegate) Delegate.CreateDelegate(typeof(MethodDelegate), method);

				AddCommand(attribute, methodDelegate);
			}

			configFilesLocation = Game.GetGameExecutePath() + "/Cfg/";
		}

		/// <summary>
		/// Adds a command to the list of commands
		/// </summary>
		/// <param name="conCommand"></param>
		/// <param name="method"></param>
		public static void AddCommand(ConCommand conCommand, MethodDelegate method)
		{
			string commandName = conCommand.Name.ToLower();

			//Make sure the command doesn't already exist
			if (Commands.ContainsKey(commandName))
			{
				Logging.Logger.Error("The command `{@CommandName}` already exists!", commandName);
				return;
			}

			Logging.Logger.Debug("Added command `{@CommandName}`.", commandName);

			//Add the command
			Commands.Add(commandName, new ConsoleCommand
			{
				CommandSummary = conCommand.Summary,
				CommandMethod = method,
				MinArgs = conCommand.MinArguments,
				MaxArgs = conCommand.MaxArguments
			});
		}

		/// <summary>
		/// Get a dictionary containing the command name, and it associated <see cref="ConsoleCommand"/>
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, ConsoleCommand> GetAllCommands()
		{
			return Commands;
		}

		/// <summary>
		/// Executes a command
		/// </summary>
		/// <param name="command"></param>
		public static void ExecuteCommand(string command)
		{
			List<string> tokens = Tokenize(command);
			if (tokens.Count < 1)
				return;

			if (Commands.TryGetValue(tokens[0].ToLower(), out ConsoleCommand conCommand))
			{
				string[] arguments = tokens.GetRange(1, tokens.Count - 1).ToArray();

				//Command min arguments
				if (conCommand.MinArgs != 0)
					if (arguments.Length <= conCommand.MinArgs - 1)
					{
						Logging.Logger.Error("Invalid arguments: More arguments are required!");
						return;
					}

				//Command max arguments
				if (conCommand.MaxArgs != 0)
					if (arguments.Length > conCommand.MaxArgs)
					{
						Logging.Logger.Error("Invalid arguments: Less arguments are required!");
						return;
					}

				//Invoke the method
				conCommand.CommandMethod.Invoke(arguments);
				return;
			}

			Logging.Logger.Error($"Unknown command: `{tokens[0]}`.");
		}

		#region Argument Parsing

		private static List<string> Tokenize(string input)
		{
			int pos = 0;
			List<string> res = new List<string>();
			int c = 0;
			while (pos < input.Length && c++ < 10000)
			{
				SkipWhite(input, ref pos);
				if (pos == input.Length)
					break;

				if (input[pos] == '"' && (pos == 0 || input[pos - 1] != '\\'))
					res.Add(ParseQuoted(input, ref pos));
				else
					res.Add(Parse(input, ref pos));
			}

			return res;
		}

		private static void SkipWhite(string input, ref int pos)
		{
			while (pos < input.Length && " \t".IndexOf(input[pos]) > -1)
			{
				pos++;
			}
		}

		private static string ParseQuoted(string input, ref int pos)
		{
			pos++;
			int startPos = pos;
			while (pos < input.Length)
			{
				if (input[pos] == '"' && input[pos - 1] != '\\')
				{
					pos++;
					return input.Substring(startPos, pos - startPos - 1);
				}

				pos++;
			}

			return input.Substring(startPos);
		}

		private static string Parse(string input, ref int pos)
		{
			int startPos = pos;
			while (pos < input.Length)
			{
				if (" \t".IndexOf(input[pos]) > -1) return input.Substring(startPos, pos - startPos);
				pos++;
			}

			return input.Substring(startPos);
		}

		#endregion

		#region File Executuion

		private static string configFilesLocation;

		[ConCommand("exec", "Executes a file", 1, 1)]
		public static void ExecuteFile(string[] args)
		{
			if (args.Length != 1)
			{
				Logging.Logger.Error("Invalid arguments!");
				return;
			}

			string fileName = args[0] + ".cfg";
			if (!File.Exists(configFilesLocation + fileName))
			{
				Logging.Logger.Error($"`{fileName}` doesn't exist! Not executing.");
				return;
			}

			string[] lines = File.ReadAllLines(configFilesLocation + fileName);
			foreach (string line in lines)
			{
				if (line.StartsWith("//")) continue;

				ExecuteCommand(line);
			}
		}

		#endregion
	}
}