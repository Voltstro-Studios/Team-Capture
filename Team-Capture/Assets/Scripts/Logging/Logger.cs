using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Team_Capture.Console;
using Team_Capture.Exceptions;
using Team_Capture.Logging.Unity;
using UnityEngine;

namespace Team_Capture.Logging
{
	/// <summary>
	///     An API to provide the ability to record events that occur in the game.
	///		<para>Try to use this API rather then Unity's <see cref="UnityEngine.Debug"/> methods.
	///		If your code needs to run in the editor, then use Unity's <see cref="UnityEngine.Debug"/> methods.</para>
	///		<para>Never use Serilog's <see cref="Log"/>, as it is never setup!</para>
	/// </summary>
	public static class Logger
	{
		private static Serilog.Core.Logger log;

		private static LoggerConfig loggerConfig;

		private static LoggingLevelSwitch level;

		/// <summary>
		///     The logger's config, can only be set while the logger isn't running
		/// </summary>
		public static LoggerConfig LoggerConfig
		{
			internal set
			{
				if (IsLoggerInitialized)
					throw new InitializationException("The logger is already initialized!");

				loggerConfig = value;
			}
			get => loggerConfig;
		}

		/// <summary>
		///     Is the logger initialized?
		///     <para>Returns true if it is</para>
		/// </summary>
		public static bool IsLoggerInitialized => log != null;

		/// <summary>
		///		Do we log debug messages
		/// </summary>
		[ConVar("cl_log_debug", "Logs debug messages", nameof(DebugLogModeCallback))]
#if UNITY_EDITOR //Default to debug logging in the editor
		internal static bool DebugLogMode = true;
#else
		public static bool DebugLogMode = false;
#endif

		/// <summary>
		///     Initializes the logger
		/// </summary>
		/// <exception cref="InitializationException"></exception>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		internal static void Init()
		{
			if (IsLoggerInitialized)
				throw new InitializationException("The logger is already initialized!");

			if (LoggerConfig == null)
				LoggerConfig = new LoggerConfig();

			Application.quitting += Shutdown;

			//Setup logging level
			level = new LoggingLevelSwitch();
			if (DebugLogMode)
				level.MinimumLevel = LogEventLevel.Debug;

			const string outPutTemplate = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			string logFileName =
				$"{loggerConfig.LogDirectory}{DateTime.Now.ToString(loggerConfig.LogFileDateTimeFormat)}.log";

			log = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(level)
				.WriteTo.Async(a => a.File(logFileName, outputTemplate: outPutTemplate,
					buffered: loggerConfig.BufferedFileWrite))
				.WriteTo.Unity3D()
				.WriteTo.Console(outPutTemplate)
				.Enrich.WithDemystifiedStackTraces()
				.CreateLogger();

			log.Debug("Logger initialized at {@Date}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
		}

		/// <summary>
		///     Shuts down the logger
		/// </summary>
		/// <exception cref="InitializationException"></exception>
		internal static void Shutdown()
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Debug("Logger shutting down at {@Date}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
			log.Dispose();
			log = null;

			loggerConfig = null;

			Application.quitting -= Shutdown;
		}

		private static void DebugLogModeCallback()
		{
			level.MinimumLevel = DebugLogMode ? LogEventLevel.Debug : LogEventLevel.Information;
		}

		#region Debug Logging

		/// <summary>
		///     Writes a debug log
		/// </summary>
		/// <param name="message"></param>
		public static void Debug(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			if(DebugLogMode)
				log.Debug(message);
		}

		/// <summary>
		///     Writes a debug log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Debug(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			if(DebugLogMode)
				log.Debug(message, values);
		}

		#endregion

		#region Information Logging

		/// <summary>
		///     Writes an information log
		/// </summary>
		/// <param name="message"></param>
		public static void Info(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Information(message);
		}

		/// <summary>
		///     Writes an information log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Info(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Information(message, values);
		}

		#endregion

		#region Warning Logging

		/// <summary>
		///     Writes a warning log
		/// </summary>
		/// <param name="message"></param>
		public static void Warn(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Warning(message);
		}

		/// <summary>
		///     Writes a warning log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Warn(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Warning(message, values);
		}

		#endregion

		#region Error Logging

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="message"></param>
		public static void Error(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(message);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Error(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(message, values);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		public static void Error(Exception exception, string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(exception, message);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Error(Exception exception, string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(exception, message, values);
		}

		#endregion
	}
}