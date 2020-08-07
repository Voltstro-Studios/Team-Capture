using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Console
{
	/// <summary>
	/// Console system for Windows
	/// </summary>
	internal class ConsoleWindows : IConsoleUI
	{
		private readonly string consoleTitle;
		private string currentLine;

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
			Logger.Info("Started Windows command line console.");
			DrawHeader();
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
			if(!System.Console.KeyAvailable)
				return;

			ConsoleKeyInfo keyInfo = System.Console.ReadKey();

			switch (keyInfo.Key)
			{
				case ConsoleKey.Enter:
					DrawInputLine("\n");
					ConsoleBackend.ExecuteCommand(currentLine);
					currentLine = "";
					DrawHeader();
					break;
				case ConsoleKey.Backspace:
					if (currentLine.Length > 0)
						currentLine = currentLine.Substring(0, currentLine.Length - 1);
					RemoveLastInput();
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

		private void DrawInputLine(string value)
		{
			System.Console.Write(value);

			DrawHeader();
		}

		private void DrawHeader()
		{
			int cursorLeft = System.Console.CursorLeft;
			int cursorTop = System.Console.CursorTop;
			ConsoleColor color = System.Console.BackgroundColor;

			System.Console.SetCursorPosition(0, 0);
			System.Console.BackgroundColor = ConsoleColor.Blue;
			string message = "Team-Capture server online.";
			System.Console.Write(message + new string(' ', System.Console.BufferWidth - message.Length));
			System.Console.BackgroundColor = color;

			System.Console.SetCursorPosition(cursorLeft, cursorTop);
		}

		private void RemoveLastInput()
		{
			System.Console.Write("\b \b");

			DrawHeader();
		}

		[DllImport("Kernel32.dll")]
		private static extern bool AllocConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool FreeConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool SetConsoleTitle(string title);
	}
}