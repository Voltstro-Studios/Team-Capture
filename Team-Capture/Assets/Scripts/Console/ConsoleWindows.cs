using UnityEngine;

namespace Console
{
	/// <summary>
	/// Console system for Windows
	/// </summary>
	internal class ConsoleWindows : IConsoleUI
	{
		public void Init()
		{
			throw new System.NotImplementedException();
		}

		public void Shutdown()
		{
			throw new System.NotImplementedException();
		}

		public void LogMessage(string message, LogType logType)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateConsole()
		{
			throw new System.NotImplementedException();
		}

		public bool IsOpen()
		{
			return true;
		}
	}
}