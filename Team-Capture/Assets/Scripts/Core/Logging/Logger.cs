﻿using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Team_Capture.Console;
using Team_Capture.Core.Logging.Unity;
using Team_Capture.Exceptions;
using UnityEngine;

namespace Team_Capture.Core.Logging
{
	/// <summary>
	///     Provides the ability to log stuff to a file and the console
	/// </summary>
	public static class Logger
	{
		private static Serilog.Core.Logger log;

		private static LoggerConfig loggerConfig;

		private static LoggingLevelSwitch level;

		/// <summary>
		///     The logger's config, can only be set while the logger isn't running
		/// </summary>
		internal static LoggerConfig LoggerConfig
		{
			set
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

		[ConVar("cl_log_debug", "Logs debug messages", nameof(DebugLogModeCallback))]
#if UNITY_EDITOR //Default to debug logging in the editor
		public static bool DebugLogMode = true;
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

			level = new LoggingLevelSwitch();

			const string outPutTemplate = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			string logFileName =
				$"{loggerConfig.LogDirectory}{DateTime.Now.ToString(loggerConfig.LogFileDateTimeFormat)}.log";

			log = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(level)
				.WriteTo.Async(a => a.File(logFileName, outputTemplate: outPutTemplate,
					buffered: loggerConfig.BufferedFileWrite))
				.WriteTo.Unity3D()
				.WriteTo.Console(outPutTemplate)
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