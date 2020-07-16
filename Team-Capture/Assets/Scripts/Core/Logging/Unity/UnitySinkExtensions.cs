using Serilog;
using Serilog.Configuration;

namespace Core.Logging.Unity
{
    public static class UnitySinkExtensions
    {
        public static LoggerConfiguration Unity3D(this LoggerSinkConfiguration loggerSinkConfiguration, string format = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}") =>
            loggerSinkConfiguration.Sink(new Unity3DLogEventSink(format));
    }
}