#if UNITY_STANDALONE_LINUX

using System;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Console
{
	public class ConsoleLinux : IConsoleUI
	{
		public void Init()
		{
			Debug.unityLogger.logEnabled = false;
			System.Console.Clear();

			Logger.Info("Started Linux command line console.");
		}

		public void Shutdown()
		{
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
			//Return if there is no key available
			if (!System.Console.KeyAvailable)
				return;

			//Read the key
			ConsoleKeyInfo keyInfo = System.Console.ReadKey();
			switch (keyInfo.Key)
			{
				//Enter in input
				case ConsoleKey.Enter:
					string value = System.Console.ReadLine();
					Logger.Info($"cmd>: {value}");
					ConsoleBackend.ExecuteCommand(value);
					
					break;
			}
		}

		public bool IsOpen()
		{
			return true;
		}
	}
}

#endif