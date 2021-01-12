using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Mirror;
using Team_Capture.Console.TypeReader;
using Team_Capture.Core;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using StringReader = Team_Capture.Console.TypeReader.StringReader;

namespace Team_Capture.Console
{
	/// <summary>
	///     The backend system for the console
	/// </summary>
	internal static class ConsoleBackend
	{
		public delegate void MethodDelegate(string[] args);

#pragma warning disable IDE0002 //For what ever reason removing these System.Reflection before the BindingFlags causes errors
		private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Static
		                                          | System.Reflection.BindingFlags.Public
		                                          | System.Reflection.BindingFlags.NonPublic;
#pragma warning restore IDE0002

		private const int HistoryCount = 50;
		private static readonly string[] History = new string[HistoryCount];
		private static int historyNextIndex;
		private static int historyIndex;

		private static Dictionary<string, ConsoleCommand> commands;

		private static Dictionary<Type, ITypeReader> typeReaders;

		/// <summary>
		///     Inits the backend of the console
		/// </summary>
		public static void InitConsoleBackend()
		{
			configFilesLocation = Game.GetGameExecutePath() + "/Cfg/";
			typeReaders = new Dictionary<Type, ITypeReader>
			{
				[typeof(string)] = new StringReader(),
				[typeof(bool)] = new BoolReader(),
				[typeof(int)] = new IntReader(),
				[typeof(float)] = new FloatReader()
			};
			commands = new Dictionary<string, ConsoleCommand>();

			RegisterCommands();
			RegisterConVars();
		}

		/// <summary>
		///     Finds all static methods with the <see cref="ConCommand" /> attribute attached to it
		///     and adds it to the list of commands
		/// </summary>
		private static void RegisterCommands()
		{
			foreach (KeyValuePair<ConCommand, MethodInfo> command in GetConCommands())
			{
				ConCommand attribute = command.Key;

				//Create the MethodDelegate from the ConCommand's method
				MethodDelegate methodDelegate;
				try
				{
					methodDelegate = (MethodDelegate) Delegate.CreateDelegate(typeof(MethodDelegate), command.Value);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred while adding the command {@Command}'s method!",
						attribute.Name);
					continue;
				}

				//Add the command
				AddCommand(new ConsoleCommand
				{
					CommandSummary = attribute.Summary,
					RunPermission = attribute.RunPermission,
					MinArgs = attribute.MinArguments,
					MaxArgs = attribute.MaxArguments,
					CommandMethod = methodDelegate
				}, attribute.Name);
			}
		}

		/// <summary>
		///     Adds all fields that have the <see cref="ConVar" /> attribute attached to it
		/// </summary>
		private static void RegisterConVars()
		{
			foreach (KeyValuePair<ConVar, FieldInfo> conVar in GetConVars())
			{
				ConVar attribute = conVar.Key;
				FieldInfo fieldInfo = conVar.Value;

				if (attribute.GraphicsOnly && Game.IsHeadless)
					continue;

				Action action = null;
				try
				{
					//Create an action if the callback string is not null
					if (attribute.Callback != null)
						action = (Action) Delegate.CreateDelegate(typeof(Action),
							fieldInfo.DeclaringType ?? throw new Exception("Field's declaring type was null!"),
							attribute.Callback);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, 
						"An error occurred while adding the ConVar {@ConVar}'s callback method!",
						attribute.Name);
					continue;
				}

				//We add the ConVar as it was a normal command, but with a custom CommandMethod
				AddCommand(new ConsoleCommand
				{
					CommandSummary = attribute.Summary,
					RunPermission = CommandRunPermission.Both,
					MinArgs = 1,
					MaxArgs = 1,
					CommandMethod = args =>
					{
						//If we have an ITypeReader for the type, then read the arg using it
						if (typeReaders.TryGetValue(fieldInfo.FieldType, out ITypeReader reader))
						{
							//Set the field and invoke the action (if not null)
							fieldInfo.SetValue(fieldInfo, reader.ReadType(args[0]));
							action?.Invoke();

							Logger.Info("'{@Attribute}' was set to '{@Value}'", attribute.Name,
								reader.ReadType(args[0]));
							return;
						}

						Logger.Error("There is no {@TypeReaderNameOf} for the Type {@Type}!", nameof(ITypeReader),
							fieldInfo.FieldType.FullName);
					}
				}, attribute.Name);
			}
		}

		/// <summary>
		///     Gets all <see cref="ConCommand" />s
		/// </summary>
		/// <returns></returns>
		internal static Dictionary<ConCommand, MethodInfo> GetConCommands()
		{
			Dictionary<ConCommand, MethodInfo> conCommands = new Dictionary<ConCommand, MethodInfo>();

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			foreach (MethodInfo method in type.GetMethods(BindingFlags))
			{
				//Ignore if the field doesn't have the ConCommand attribute, but if it does, get it
				if (!(Attribute.GetCustomAttribute(method, typeof(ConCommand)) is ConCommand attribute))
					continue;

				//If this ConCommand is graphics only, and the game is headless, then we ignore adding it
				if (attribute.GraphicsModeOnly && Game.IsHeadless)
					continue;

				conCommands.Add(attribute, method);
			}

			return conCommands;
		}

		/// <summary>
		///     Gets all <see cref="ConVar" />s
		/// </summary>
		/// <returns></returns>
		internal static Dictionary<ConVar, FieldInfo> GetConVars()
		{
			Dictionary<ConVar, FieldInfo> conVars = new Dictionary<ConVar, FieldInfo>();

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags))
			{
				//Ignore if the field doesn't have the ConVar attribute, but if it does, get it
				if (!(Attribute.GetCustomAttribute(fieldInfo, typeof(ConVar)) is ConVar attribute))
					continue;

				conVars.Add(attribute, fieldInfo);
			}

			return conVars;
		}

		/// <summary>
		///     Adds a command to the list of commands
		/// </summary>
		/// <param name="conCommand"></param>
		/// <param name="commandName"></param>
		private static void AddCommand(ConsoleCommand conCommand, string commandName)
		{
			//Make sure the command doesn't already exist
			if (commands.ContainsKey(commandName))
			{
				Logger.Error("The command {@CommandName} already exists in the command list!", commandName);
				return;
			}

			Logger.Debug("Added command {@CommandName}", commandName);

			//Add the command
			commands.Add(commandName, conCommand);
		}

		/// <summary>
		///     Get a dictionary containing the command name, and it associated <see cref="ConsoleCommand" />
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, ConsoleCommand> GetAllCommands()
		{
			return commands;
		}

		/// <summary>
		///     Executes a command
		/// </summary>
		/// <param name="command"></param>
		public static void ExecuteCommand(string command)
		{
			List<string> tokens = Tokenize(command);
			if (tokens.Count < 1)
				return;

			if (commands.TryGetValue(tokens[0].ToLower(), out ConsoleCommand conCommand))
			{
				//Get the arguments that were inputted
				string[] arguments = tokens.GetRange(1, tokens.Count - 1).ToArray();

				//Make sure run permissions are all good
				switch (conCommand.RunPermission)
				{
					//The command has a RunPermission of ServerOnly, so this command can only be executed in server mode
					case CommandRunPermission.ServerOnly when NetworkManager.singleton == null ||
					                                          NetworkManager.singleton.mode !=
					                                          NetworkManagerMode.ServerOnly:
						Logger.Error("The command {@Command} can only be run in server mode!", tokens[0].ToLower());
						return;

					//The command has a RunPermission of ClientOnly, so this command can only be executed as a client
					case CommandRunPermission.ClientOnly:
					{
						if (NetworkManager.singleton != null)
							if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
							{
								Logger.Error("The command {@Command} can only be run in client/offline mode!",
									tokens[0].ToLower());
								return;
							}

						break;
					}
				}

				//Command min arguments
				if (conCommand.MinArgs != 0)
					if (arguments.Length <= conCommand.MinArgs - 1)
					{
						Logger.Error("Invalid arguments: More arguments are required!");
						return;
					}

				//Command max arguments
				if (conCommand.MaxArgs != 0)
					if (arguments.Length > conCommand.MaxArgs)
					{
						Logger.Error("Invalid arguments: Less arguments are required!");
						return;
					}

				//Invoke the method
				try
				{
					conCommand.CommandMethod.Invoke(arguments);
					History[historyNextIndex % HistoryCount] = command;
					historyNextIndex++;
					historyIndex = historyNextIndex;
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred while handling a command!");
				}

				return;
			}

			Logger.Error($"Unknown command: `{tokens[0]}`.");
		}

		/// <summary>
		///     Tries to auto complete a prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static string AutoComplete(string prefix)
		{
			List<string> possibleMatches = new List<string>();

			//Go through all commands, and find if they start with the prefix
			foreach (KeyValuePair<string, ConsoleCommand> command in commands)
			{
				string name = command.Key;
				if (!name.StartsWith(prefix, true, null))
					continue;

				possibleMatches.Add(name);
			}

			//No commands that start with the prefix where found, so just return
			if (possibleMatches.Count == 0)
				return prefix;

			//Look for longest common prefix
			int lcp = possibleMatches[0].Length;
			for (int i = 0; i < possibleMatches.Count - 1; i++)
				lcp = Mathf.Min(lcp, CommonPrefix(possibleMatches[i], possibleMatches[i + 1]));

			prefix += possibleMatches[0].Substring(prefix.Length, lcp - prefix.Length);
			if (possibleMatches.Count <= 1) return prefix;

			//Build a list of all possible matches
			StringBuilder sb = new StringBuilder();
			sb.Append("Possible matches:\n");

			for (int i = 0; i < possibleMatches.Count; i++)
			{
				if (i != 0)
					sb.Append("\n");
				sb.Append(possibleMatches[i]);
			}

			Logger.Info(sb.ToString());
			return prefix;
		}

		public static string HistoryUp(string current)
		{
			if (historyIndex == 0 || historyNextIndex - historyIndex >= HistoryCount - 1)
				return "";

			if (historyIndex == historyNextIndex) History[historyIndex % HistoryCount] = current;

			historyIndex--;

			return History[historyIndex % HistoryCount];
		}

		public static string HistoryDown()
		{
			if (historyIndex == historyNextIndex)
				return "";

			historyIndex++;

			return History[historyIndex % HistoryCount];
		}

		/// <summary>
		///     Returns length of largest common prefix of two strings
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static int CommonPrefix(string a, string b)
		{
			int minl = Mathf.Min(a.Length, b.Length);
			for (int i = 1; i <= minl; i++)
				if (!a.StartsWith(b.Substring(0, i), true, null))
					return i - 1;

			return minl;
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

		[ConCommand("exec", "Executes a file", CommandRunPermission.Both, 1, 1)]
		public static void ExecuteFileCommand(string[] args)
		{
			if (args.Length != 1)
			{
				Logger.Error("Invalid arguments!");
				return;
			}

			string fileName = args[0] + ".cfg";
			if (!File.Exists(configFilesLocation + fileName))
			{
				Logger.Error($"`{fileName}` doesn't exist! Not executing.");
				return;
			}

			string[] lines = File.ReadAllLines(configFilesLocation + fileName);
			foreach (string line in lines)
			{
				if (line.StartsWith("//") || line.StartsWith(" ")) continue;

				ExecuteCommand(line);
			}
		}

		#endregion
	}
}