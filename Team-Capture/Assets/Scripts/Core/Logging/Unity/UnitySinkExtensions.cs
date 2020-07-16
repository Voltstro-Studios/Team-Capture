using System;
using Serilog;
using Serilog.Configuration;

namespace Core.Logging.Unity
{
    public static class UnitySinkExtensions
    {
        public static LoggerConfiguration Unity3D(this LoggerSinkConfiguration loggerSinkConfiguration, IFormatProvider formatProvider = null) =>
            loggerSinkConfiguration.Sink(new Unity3DLogEventSink(formatProvider));
    }
}