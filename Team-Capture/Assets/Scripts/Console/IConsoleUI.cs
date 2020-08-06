using UnityEngine;

namespace Console
{
	/// <summary>
	/// The interface for a console
	/// </summary>
	internal interface IConsoleUI
	{
		/// <summary>
		/// Initializes the console UI
		/// </summary>
		void Init();

		/// <summary>
		/// Shutdowns the console UI
		/// </summary>
		void Shutdown();

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="logType"></param>
		void LogMessage(string message, LogType logType);

		/// <summary>
		/// Called every frame
		/// </summary>
		void UpdateConsole();

		/// <summary>
		/// If the console is opened or not
		/// </summary>
		/// <returns></returns>
		bool IsOpen();
	}
}