using System;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Core.Logging.Unity
{
    public sealed class Unity3DLogEventSink : ILogEventSink
    {
        private readonly IFormatProvider formatProvider;

        public Unity3DLogEventSink(IFormatProvider formatProvider)
        {
            this.formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage(formatProvider);

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