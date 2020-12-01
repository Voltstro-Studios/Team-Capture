#if UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Runtime.InteropServices;
using Mirror;
using SceneManagement;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Console
{
	/// <summary>
	///     Console system for Windows
	/// </summary>
	internal class ConsoleWindows : IConsoleUI
	{
		private readonly string consoleTitle;
		private string currentLine;

		private float nextHeaderUpdateTime;

		internal ConsoleWindows(string consoleTitle)
		{
			this.consoleTitle = consoleTitle;
		}

		public void Init()
		{
			//Allocate a new console for us
			AllocConsole();

			//Set the title, color, out, in, buffer
			SetConsoleTitle(consoleTitle);
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.Clear();
			System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()) {AutoFlush = true});
			System.Console.SetIn(new StreamReader(System.Console.OpenStandardInput()));
			System.Console.BufferHeight = System.Console.WindowHeight;
			System.Console.Write("\n");
			currentLine = "";

			Logger.Info("Started Windows command line console.");

			//Draw our header for the first
			DrawHeader();
			TCScenesManager.OnSceneLoadedEvent += scene => DrawHeader(); //Re-draw it if the scene changes
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
			//If enough time has past since last header draw time, re-draw it
			if (Time.time >= nextHeaderUpdateTime)
				DrawHeader();

			//Return if there is no key available
			if (!System.Console.KeyAvailable)
				return;

			//Read the key
			ConsoleKeyInfo keyInfo = System.Console.ReadKey();
			switch (keyInfo.Key)
			{
				//Enter in input
				case ConsoleKey.Enter:
					DrawInputLine("\n");
					ConsoleBackend.ExecuteCommand(currentLine);
					currentLine = "";

					break;

				//Remove last input
				case ConsoleKey.Backspace:
					if (currentLine.Length > 0)
						currentLine = currentLine.Substring(0, currentLine.Length - 1);
					RemoveLastInput();

					break;

				//Attempt to auto complete
				case ConsoleKey.Tab:
					ClearLine();
					currentLine = ConsoleBackend.AutoComplete(currentLine);
					System.Console.Write(currentLine);

					break;

				//Go up in history of commands
				case ConsoleKey.PageUp:
				case ConsoleKey.UpArrow:
					ClearLine();
					currentLine = ConsoleBackend.HistoryUp(currentLine);
					System.Console.Write(currentLine);

					break;

				//Go back in history of commands
				case ConsoleKey.PageDown:
				case ConsoleKey.DownArrow:
					ClearLine();
					currentLine = ConsoleBackend.HistoryDown();
					System.Console.Write(currentLine);

					break;

				//Enter in key char
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
			//Calculate next header update time
			nextHeaderUpdateTime = Time.time + 2f;

			//Get current cursor and color
			int cursorLeft = System.Console.CursorLeft;
			int cursorTop = System.Console.CursorTop;
			ConsoleColor color = System.Console.BackgroundColor;

			//Set the cursor position to the top of the console
			System.Console.SetCursorPosition(0, 0);

			//And set the background color
			System.Console.BackgroundColor = ConsoleColor.Blue;

			//Get if our server is offline or online
			string serverOnline = "Offline";
			if (NetworkManager.singleton != null)
				if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
					serverOnline = "Online";

			//Final message
			string message = $"Team-Capture server: {serverOnline} - {TCScenesManager.GetActiveScene().displayName}";
			System.Console.Write(message + new string(' ', System.Console.BufferWidth - message.Length));

			//Reset everything to how it was before
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

#endif