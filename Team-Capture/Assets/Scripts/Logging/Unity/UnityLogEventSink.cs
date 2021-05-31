// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using UnityEngine;

namespace Team_Capture.Logging.Unity
{
	internal static class UnitySinkExtensions
	{
		public static LoggerConfiguration Unity(this LoggerSinkConfiguration loggerSinkConfiguration,
			string format = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
		{
			return loggerSinkConfiguration.Sink(new UnityLogEventSink(format));
		}
	}

	internal sealed class UnityLogEventSink : ILogEventSink
	{
		private readonly ITextFormatter formatProvider;

		public UnityLogEventSink(string messageFormat)
		{
			formatProvider = new MessageTemplateTextFormatter(messageFormat);
		}

		public void Emit(LogEvent logEvent)
		{
			StringWriter writer = new StringWriter();
			formatProvider.Format(logEvent, writer);
			string message = writer.ToString();

			switch (logEvent.Level)
			{
				case LogEventLevel.Verbose:
					Debug.Log(message);
					break;
				case LogEventLevel.Debug:
					Debug.Log(message);
					break;
				case LogEventLevel.Information:
					Debug.Log(message);
					break;
				case LogEventLevel.Warning:
					Debug.LogWarning(message);
					break;
				case LogEventLevel.Error:
					Debug.LogError(message);
					break;
				case LogEventLevel.Fatal:
					Debug.LogError(message);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}