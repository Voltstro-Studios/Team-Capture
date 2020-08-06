using System;
using System.IO;
using System.Runtime.InteropServices;
using Core;
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
		private IntPtr foregroundWindow;
		private float resetWindowTime;
		private TextWriter previousOut;

		private string prompt;
		private string currentLine;

		internal ConsoleWindows(string consoleTitle)
		{
			this.consoleTitle = consoleTitle;
		}

		public void Init()
		{
			if (!AttachConsole(0xffffffff))
			{
				foregroundWindow = GetForegroundWindow();
				resetWindowTime = Time.time + 1;

				AllocConsole();
			}
			previousOut = System.Console.Out;

			SetConsoleTitle(consoleTitle);
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.Clear();
			System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()) {AutoFlush = true});
			Logger.Info("Started Windows command line console.");
		}

		public void Shutdown()
		{
			System.Console.SetOut(previousOut);
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
			if (foregroundWindow != IntPtr.Zero && Time.time > resetWindowTime)
			{
				ShowWindow(foregroundWindow, 9);
				SetForegroundWindow(foregroundWindow);
				foregroundWindow = IntPtr.Zero;
			}
		}

		public bool IsOpen()
		{
			return true;
		}

		[DllImport("Kernel32.dll")]
		private static extern bool AttachConsole(uint processId);

		[DllImport("Kernel32.dll")]
		private static extern bool AllocConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool FreeConsole();

		[DllImport("Kernel32.dll")]
		private static extern bool SetConsoleTitle(string title);

		[DllImport("Kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hwnd);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	}
}