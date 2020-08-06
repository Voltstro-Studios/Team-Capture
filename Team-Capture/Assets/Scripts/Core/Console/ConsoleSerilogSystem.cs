using System;
using Console;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Core.Console
{
	public static class ConsoleSerilogSystem
	{
		public static LoggerConfiguration TCConsoleSystem(this LoggerSinkConfiguration loggerSinkConfiguration) =>
			loggerSinkConfiguration.Sink(new TCConsoleSystem());
	}

	public sealed class TCConsoleSystem : ILogEventSink
	{
		public void Emit(LogEvent logEvent)
		{
			string message = logEvent.RenderMessage(null);
			IConsoleUI console = ConsoleSetup.ConsoleUI;
			if(console == null)
				return;

			switch (logEvent.Level)
			{
				case LogEventLevel.Verbose:
					console.LogMessage(message, LogType.Log);
					break;
				case LogEventLevel.Debug:
					console.LogMessage(message, LogType.Assert);
					break;
				case LogEventLevel.Information:
					console.LogMessage(message, LogType.Log);
					break;
				case LogEventLevel.Warning:
					console.LogMessage(message, LogType.Warning);
					break;
				case LogEventLevel.Error:
					console.LogMessage(message, LogType.Error);
					break;
				case LogEventLevel.Fatal:
					console.LogMessage(message, LogType.Error);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}