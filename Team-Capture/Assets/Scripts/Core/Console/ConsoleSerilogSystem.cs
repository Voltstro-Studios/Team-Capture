using System;
using Core.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using UI;

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
			ConsoleGUI console = ConsoleGUI.Instance;
			if(console == null)
				return;

			switch (logEvent.Level)
			{
				case LogEventLevel.Verbose:
					console.LoggerLog(message, LogVerbosity.Debug);
					break;
				case LogEventLevel.Debug:
					console.LoggerLog(message, LogVerbosity.Debug);
					break;
				case LogEventLevel.Information:
					console.LoggerLog(message, LogVerbosity.Info);
					break;
				case LogEventLevel.Warning:
					console.LoggerLog(message, LogVerbosity.Warn);
					break;
				case LogEventLevel.Error:
					console.LoggerLog(message, LogVerbosity.Error);
					break;
				case LogEventLevel.Fatal:
					console.LoggerLog(message, LogVerbosity.Error);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}