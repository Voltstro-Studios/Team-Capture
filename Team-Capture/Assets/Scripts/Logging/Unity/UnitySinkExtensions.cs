using Serilog;
using Serilog.Configuration;

namespace Team_Capture.Logging.Unity
{
	internal static class UnitySinkExtensions
	{
		public static LoggerConfiguration Unity3D(this LoggerSinkConfiguration loggerSinkConfiguration,
			string format = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
		{
			return loggerSinkConfiguration.Sink(new Unity3DLogEventSink(format));
		}
	}
}