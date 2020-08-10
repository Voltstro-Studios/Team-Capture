using System;
using System.IO;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using UnityEngine;

namespace Console
{
	public static class ConsoleSerilogSystem
	{
		public static LoggerConfiguration TCConsoleSystem(this LoggerSinkConfiguration loggerSinkConfiguration, string messageFormat) =>
			loggerSinkConfiguration.Sink(new TCConsoleSystem(messageFormat));
	}

	public sealed class TCConsoleSystem : ILogEventSink
	{
		private readonly ITextFormatter formatter;

		public TCConsoleSystem(string messageFormat)
		{
			formatter = new MessageTemplateTextFormatter(messageFormat);
		}

		public void Emit(LogEvent logEvent)
		{
			StringWriter writer = new StringWriter();
			formatter.Format(logEvent, writer);
			string message = writer.ToString();
			message = message.Remove(message.Length - 2, 2);

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