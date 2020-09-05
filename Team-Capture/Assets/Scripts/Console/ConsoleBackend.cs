using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Attributes;
using Core;
using Delegates;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Console
{
	public static class ConsoleBackend
	{
		private const string SplashScreenResourceFile = "Resources/console-splashscreen.txt";

		private static readonly Dictionary<string, ConsoleCommand> Commands = new Dictionary<string, ConsoleCommand>();

		private const int HistoryCount = 50;
		private static readonly string[] History = new string[HistoryCount];
		private static int historyNextIndex;
		private static int historyIndex;

		/// <summary>
		/// Finds all static methods with the <see cref="ConCommand"/> attribute attached to it
		/// and adds it to the list of commands
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
				Logger.Error("The command {@CommandName} already exists in the command list!", commandName);
				return;
			}

			Logger.Debug("Added command {@CommandName}", commandName);

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
					Logger.Error("An error occured! {@Exception}", ex);
				}

				return;
			}

			Logger.Error($"Unknown command: `{tokens[0]}`.");
		}

		/// <summary>
		/// Tries to auto complete a prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static string AutoComplete(string prefix)
		{
			List<string> possibleMatches = new List<string>();

			foreach (KeyValuePair<string, ConsoleCommand> command in Commands)
			{
				string name = command.Key;
				if(!name.StartsWith(prefix, true, null))
					continue;
				possibleMatches.Add(name);
			}

			if (possibleMatches.Count == 0)
				return prefix;

			// Look for longest common prefix
			int lcp = possibleMatches[0].Length;
			for (int i = 0; i < possibleMatches.Count - 1; i++)
			{
				lcp = Mathf.Min(lcp, CommonPrefix(possibleMatches[i], possibleMatches[i + 1]));
			}

			prefix += possibleMatches[0].Substring(prefix.Length, lcp - prefix.Length);
			if (possibleMatches.Count > 1)
			{
				// write list of possible completions
				foreach (string t in possibleMatches)
					Logger.Info(t);
			}
			else
			{
				prefix += " ";
			}

			return prefix;
		}

		public static string HistoryUp(string current)
		{
			if (historyIndex == 0 || historyNextIndex - historyIndex >= HistoryCount - 1)
				return "";

			if (historyIndex == historyNextIndex)
			{
				History[historyIndex % HistoryCount] = current;
			}

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
		/// Returns length of largest common prefix of two strings
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static int CommonPrefix(string a, string b)
		{
			int minl = Mathf.Min(a.Length, b.Length);
			for (int i = 1; i <= minl; i++)
			{
				if (!a.StartsWith(b.Substring(0, i), true, null))
					return i - 1;
			}
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

		[ConCommand("exec", "Executes a file", 1, 1)]
		public static void ExecuteFile(string[] args)
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
				if (line.StartsWith("//")) continue;

				ExecuteCommand(line);
			}
		}

		#endregion

		#region Additional Commands

		[ConCommand("asciiart", "Shows Team-Capture ascii art")]
		public static void AsciiArtCmd(string[] args)
		{
			ShowAsciiArt();
		}

		[ConCommand("splashmessage", "Shows a random splash message")]
		public static void SplashMessage(string[] args)
		{
			ShowSplashMessage();
		}

		#endregion

		/// <summary>
		/// Shows Team-Capture ascii art
		/// </summary>
		public static void ShowAsciiArt()
		{
			//Ascii art, fuck you
			const string asciiArt = @"
___________                    
\__    ___/___ _____    _____  
  |    |_/ __ \\__  \  /     \ 
  |    |\  ___/ / __ \|  Y Y  \
  |____| \___  >____  /__|_|  /
             \/     \/      \/ 
	_________                __                        
	\_   ___ \_____  _______/  |_ __ _________   ____  
	/    \  \/\__  \ \____ \   __\  |  \_  __ \_/ __ \ 
	\     \____/ __ \|  |_> >  | |  |  /|  | \/\  ___/ 
	 \______  (____  /   __/|__| |____/ |__|    \___  >
	        \/     \/|__|                           \/ 
";
			Logger.Info(asciiArt);
		}

		/// <summary>
		/// Shows a random splash message
		/// </summary>
		public static void ShowSplashMessage()
		{
			//Random splash message
			string splashMessagesPath = $"{Game.GetGameExecutePath()}/{SplashScreenResourceFile}";
			if (File.Exists(splashMessagesPath))
			{
				string[] lines = File.ReadAllLines(splashMessagesPath);

				//Select random number
				int index = UnityEngine.Random.Range(0, lines.Length);
				Logger.Info($"	{lines[index]}");
			}
		}
	}
}