using System;
using System.IO;
using System.Runtime.InteropServices;
using Core;
using Mirror;
using SceneManagement;
using UnityEngine;
using Logger = Core.Logging.Logger;
using Random = UnityEngine.Random;

namespace Console
{
	/// <summary>
	/// Console system for Windows
	/// </summary>
	internal class ConsoleWindows : IConsoleUI
	{
		private readonly string consoleTitle;
		private string currentLine;

		private const string SplashScreenResourceFile = "Resources/console-splashscreen.txt";

		private float nextHeaderUpdateTime = 0;

		internal ConsoleWindows(string consoleTitle)
		{
			this.consoleTitle = consoleTitle;
		}

		public void Init()
		{
			AllocConsole();

			SetConsoleTitle(consoleTitle);
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.Clear();
			System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()) {AutoFlush = true});
			System.Console.SetIn(new StreamReader(System.Console.OpenStandardInput()));
			System.Console.BufferHeight = System.Console.WindowHeight;
			System.Console.Write("\n");
			currentLine = "";

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
			System.Console.WriteLine(asciiArt);

			//Random splash message
			string splashMessagesPath = $"{Game.GetGameExecutePath()}/{SplashScreenResourceFile}";
			if (File.Exists(splashMessagesPath))
			{
				string[] lines = File.ReadAllLines(splashMessagesPath);

				//Select random number
				int index = Random.Range(0, lines.Length);
				System.Console.WriteLine($"		{lines[index]}");
				System.Console.WriteLine("");
			}

			Logger.Info("Started Windows command line console.");

			DrawHeader();
			TCScenesManager.OnSceneLoadedEvent += scene => DrawHeader();
		}

		public void Shutdown()
		{
			FreeConsole();
		}

		public void LogMessage(string message, LogType logType)
		{
			switch (logType)
			{
				case LogType.Exception:
				case LogType.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					System.Console.WriteLine(message);
					System.Console.ResetColor();
					break;
				case LogType.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					System.Console.WriteLine(message);
					System.Console.ResetColor();
					break;
				case LogType.Assert:
				case LogType.Log:
					System.Console.WriteLine(message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
			}
		}

		public void UpdateConsole()
		{
			if (Time.time >= nextHeaderUpdateTime)
				DrawHeader();

			if(!System.Console.KeyAvailable)
				return;

			ConsoleKeyInfo keyInfo = System.Console.ReadKey();

			switch (keyInfo.Key)
			{
				case ConsoleKey.Enter:
					DrawInputLine("\n");
					ConsoleBackend.ExecuteCommand(currentLine);
					currentLine = "";

					break;
				case ConsoleKey.Backspace:
					if (currentLine.Length > 0)
						currentLine = currentLine.Substring(0, currentLine.Length - 1);
					RemoveLastInput();
					break;
				case ConsoleKey.Tab:
					ClearLine();
					currentLine = ConsoleBackend.AutoComplete(currentLine);
					System.Console.Write(currentLine);

					break;
				case ConsoleKey.PageUp:
				case ConsoleKey.UpArrow:
					ClearLine();
					currentLine = ConsoleBackend.HistoryUp(currentLine);
					System.Console.Write(currentLine);

					break;
				case ConsoleKey.PageDown:
				case ConsoleKey.DownArrow:
					ClearLine();
					currentLine = ConsoleBackend.HistoryDown();
					System.Console.Write(currentLine);

					break;
				default:
					currentLine += keyInfo.KeyChar;
					DrawInputLine(keyInfo.KeyChar.ToString());
					break;
			}
		}

		public bool IsOpen()
		{
			return true;
		}

		private static void DrawInputLine(string value)
		{
			System.Console.Write(value);
		}

		private void DrawHeader()
		{
			nextHeaderUpdateTime = Time.time + 2f;

			int cursorLeft = System.Console.CursorLeft;
			int cursorTop = System.Console.CursorTop;
			ConsoleColor color = System.Console.BackgroundColor;

			System.Console.SetCursorPosition(0, 0);
			System.Console.BackgroundColor = ConsoleColor.Blue;

			string serverOnline = "Offline";
			if(NetworkManager.singleton != null)
				if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
					serverOnline = "Online";

			string message = $"Team-Capture server: {serverOnline} - {TCScenesManager.GetActiveScene().displayName}";
			System.Console.Write(message + new string(' ', System.Console.BufferWidth - message.Length));
			System.Console.BackgroundColor = color;

			System.Console.SetCursorPosition(cursorLeft, cursorTop);
		}

		private static void RemoveLastInput()
		{
			System.Console.Write("\b \b");
		}

		private static void ClearLine()
		{
			System.Console.CursorLeft = 0;
			System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
			System.Console.CursorLeft = 0;
		}

		[DllImport("Kernel32.dll")]
		private static extern bool AllocConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool FreeConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool SetConsoleTitle(string title);
	}
}