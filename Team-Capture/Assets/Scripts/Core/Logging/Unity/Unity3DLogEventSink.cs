using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using UnityEngine;

namespace Core.Logging.Unity
{
    internal sealed class Unity3DLogEventSink : ILogEventSink
    {
        private readonly ITextFormatter formatProvider;

        public Unity3DLogEventSink(string messageFormat)
        {
	        this.formatProvider = new MessageTemplateTextFormatter(messageFormat);
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