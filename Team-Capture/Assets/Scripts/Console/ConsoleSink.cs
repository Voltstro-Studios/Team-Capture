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

namespace Team_Capture.Console
{
    internal static class ConsoleSink
    {
        public static LoggerConfiguration Console(this LoggerSinkConfiguration loggerSinkConfiguration,
            string messageFormat)
        {
            return loggerSinkConfiguration.Sink(new ConsoleSerilogSink(messageFormat));
        }
    }

    internal sealed class ConsoleSerilogSink : ILogEventSink
    {
        private readonly ITextFormatter formatter;

        public ConsoleSerilogSink(string messageFormat)
        {
            formatter = new MessageTemplateTextFormatter(messageFormat);
        }

        public void Emit(LogEvent logEvent)
        {
            StringWriter writer = new();
            formatter.Format(logEvent, writer);
            string message = writer.ToString();
            message = message.Remove(message.Length - 1, 1);

            IConsoleUI console = ConsoleSetup.ConsoleUI;
            if (console == null)
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