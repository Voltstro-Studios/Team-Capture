// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Diagnostics;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Team_Capture.Console;
using Team_Capture.Exceptions;
using Team_Capture.Logging.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using ILogger = UnityEngine.ILogger;

namespace Team_Capture.Logging
{
	/// <summary>
	///     An API to provide the ability to record events that occur in the game.
	///     <para>
	///         Try to use this API rather then Unity's <see cref="UnityEngine.Debug" /> methods.
	///         If your code needs to run in the editor, then use Unity's <see cref="UnityEngine.Debug" /> methods.
	///     </para>
	///     <para>Never use Serilog's <see cref="Log" />, as it is never setup!</para>
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

        public static ILogger UnityLogger { get; private set; }

        /// <summary>
        ///     Is the logger initialized?
        ///     <para>Returns true if it is</para>
        /// </summary>
        public static bool IsLoggerInitialized => log != null;

        /// <summary>
        ///     Do we log debug messages
        /// </summary>
        [ConVar("cl_log_debug", "Logs debug messages", nameof(DebugLogModeCallback))]
#if UNITY_EDITOR //Default to debug logging in the editor
        internal static bool DebugLogMode = true;
#else
		internal static bool DebugLogMode = false;
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

            UnityLogger = new TCUnityLogger();

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
                .WriteTo.Unity()
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

            log.Debug("Logger shutting down at {Date}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
            log.Dispose();
            log = null;

            loggerConfig = null;

            Application.quitting -= Shutdown;
        }

        [Preserve]
        private static void DebugLogModeCallback()
        {
            level.MinimumLevel = DebugLogMode ? LogEventLevel.Debug : LogEventLevel.Information;
        }

        private static void CheckInitialization()
        {
#if !UNITY_EDITOR
            if (!IsLoggerInitialized)
                throw new InitializationException("The logger isn't initialized!");
#endif
        }


        [Conditional("UNITY_EDITOR")]
        private static void LogIfUnInitialized(string message, LogEventLevel logLevel)
        {
#if UNITY_EDITOR
            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                case LogEventLevel.Information:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogEventLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
#endif
        }

        #region Debug Logging

        /// <summary>
        ///     Writes a debug log
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            CheckInitialization();
            LogIfUnInitialized(message, LogEventLevel.Debug);
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            if (DebugLogMode)
                log.Debug(message);
        }

        /// <summary>
        ///     Writes a debug log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public static void Debug(string message, params object[] values)
        {
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            if (DebugLogMode)
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
            CheckInitialization();
            LogIfUnInitialized(message, LogEventLevel.Information);
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            log.Information(message);
        }

        /// <summary>
        ///     Writes an information log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public static void Info(string message, params object[] values)
        {
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

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
            CheckInitialization();
            LogIfUnInitialized(message, LogEventLevel.Warning);
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            log.Warning(message);
        }

        /// <summary>
        ///     Writes a warning log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public static void Warn(string message, params object[] values)
        {
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

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
            CheckInitialization();
            LogIfUnInitialized(message, LogEventLevel.Error);
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            log.Error(message);
        }

        /// <summary>
        ///     Writes an error log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public static void Error(string message, params object[] values)
        {
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            log.Error(message, values);
        }

        /// <summary>
        ///     Writes an error log
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public static void Error(Exception exception, string message)
        {
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

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
            CheckInitialization();
#if UNITY_EDITOR
            if(!IsLoggerInitialized)
                return;
#endif

            log.Error(exception, message, values);
        }

        #endregion
    }
}