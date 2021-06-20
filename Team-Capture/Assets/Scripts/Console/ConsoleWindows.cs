// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

#if UNITY_STANDALONE_WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console
{
	/// <summary>
	///     Console system for Windows
	/// </summary>
	[Preserve]
	internal class ConsoleWindows : IConsoleUI
	{
		private readonly string consoleTitle;
		private bool isRunning;

		internal ConsoleWindows(string consoleTitle)
		{
			this.consoleTitle = consoleTitle;
		}

		public void Init()
		{
			Debug.unityLogger.logEnabled = false;
			
			AllocConsole();
			InitializeOutStream();
			InitializeInStream();
			isRunning = true;
			_ = Task.Run(HandleInputs);

			Logger.Info("Started Windows command line console.");
		}

		public void Shutdown()
		{
			isRunning = false;
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

		private Task HandleInputs()
		{
			while (isRunning)
			{
				string input = System.Console.ReadLine();
				ConsoleBackend.ExecuteCommand(input);
			}
			
			return Task.CompletedTask;
		}

		public void UpdateConsole()
		{
		}

		public bool IsOpen()
		{
			return true;
		}
		
		//"Borrowed" from: https://stackoverflow.com/a/48864902 

		private static void InitializeOutStream()
		{
			FileStream fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
			if (fs == null) return;
			
			StreamWriter writer = new StreamWriter(fs) { AutoFlush = true };
			System.Console.SetOut(writer);
			System.Console.SetError(writer);
		}

		private static void InitializeInStream()
		{
			FileStream fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
			if (fs != null)
			{
				System.Console.SetIn(new StreamReader(fs));
			}
		}

		private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
			FileAccess dotNetFileAccess)
		{
			SafeFileHandle file = new SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
			if (file.IsInvalid) return null;
			
			FileStream fs = new FileStream(file, dotNetFileAccess);
			return fs;
		}
		
		[DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern int AllocConsole();

		[DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern IntPtr CreateFileW(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile
		);

		private const uint GENERIC_WRITE = 0x40000000;
		private const uint GENERIC_READ = 0x80000000;
		private const uint FILE_SHARE_READ = 0x00000001;
		private const uint FILE_SHARE_WRITE = 0x00000002;
		private const uint OPEN_EXISTING = 0x00000003;
		private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
	}
}

#endif