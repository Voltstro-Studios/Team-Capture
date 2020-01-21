using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.Logger
{
	public static class Logger
	{
		/// <summary>
		/// The <see cref="StreamWriter"/> that represents our log file
		/// </summary>
		private static StreamWriter logStream;

		/// <summary>
		/// A queue containing all of our messages that we need to write to our log
		/// </summary>
		private static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

		/// <summary>
		/// This bool signals to our logger when it's time to 'shut down'
		/// </summary>
		private static bool endLogger;

		/// <summary>
		/// The directory to log to
		/// </summary>
		private static string logDirectory;

		/// <summary>
		/// The final log name
		/// </summary>
		private static string finalLogName;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			//Now create the new log file, as long as we don't already have it open
			if (logStream == null)
			{
				//Gets the data path then backs out one
				logDirectory = Directory.GetParent(Application.dataPath).FullName + "/Logs/";

				if (!Directory.Exists(logDirectory))
					Directory.CreateDirectory(logDirectory);

				#region Logger Start Messages

#if !UNITY_EDITOR //Only add this stuff if it isn't a Unity editor build
	            Messages.Enqueue($"{Application.productName} version {Application.version}, Unity version {Application.unityVersion} starting at {GetCurrentTime()}");
                Messages.Enqueue($"Using {SystemInfo.graphicsDeviceType} rendering API.");
                Messages.Enqueue($@"For a more detailed log, goto {Application.persistentDataPath}/output_log.txt");
#endif

				#endregion

				finalLogName = CalculateFinalLogName();

				logStream = File.CreateText(logDirectory + "latest.log");
				logStream.AutoFlush = true;

				//Now we set up our disposer to execute when the game is closed or the editor stops playing
				Application.quitting += LoggerClose;

				Task.Run(() => WriteMessages().GetAwaiter().GetResult());
			}
			//If we already have a log stream, just log a message
			else
			{
				Log("Log stream already open...", LogVerbosity.Debug);
			}
		}

		/// <summary>
		/// This automatically writes our log messages to our stream writer in a thread safe manner
		/// </summary>
		private static async Task WriteMessages()
		{
			//Loop until we need to quit
			while (!endLogger)
			{
				if (Messages.IsEmpty)
				{
					//Delay by 100ms so the CPU slows doing this task down by a far amount
					await Task.Delay(100);
					continue;
				}

				//If we have a message to write, dequeue it and write it to the log
				if (Messages.TryDequeue(out string message))
					logStream.WriteLine(message);
			}
		}

		/// <summary>
		/// Cleans up any left over resources
		/// </summary>
		private static void LoggerClose()
		{
			//Signal to our logging task that it's time to end
			endLogger = true;

			while (!Messages.IsEmpty)
			{
				Messages.TryDequeue(out string msg);
				logStream.WriteLine(msg);
			}

			string endLogMessage = $"Goodbye! Logger shutdown at {GetCurrentTime()}";
			logStream.WriteLine(endLogMessage);
			Debug.Log(endLogMessage); //Log it to the Unity console so we know the logger actually shutdown-ed

			logStream.Dispose();

			File.Copy(logDirectory + "latest.log", logDirectory + finalLogName);
		}

		/// <summary>
		/// Logs a string to the log file
		/// </summary>
		/// <param name="message">The string to log to the log file</param>
		/// <param name="verbosity"></param>
		[MethodImpl(MethodImplOptions.NoInlining)] //Don't inline, to preserve stack traces
		public static void Log(string message, LogVerbosity verbosity = LogVerbosity.Info)
		{
			if (logStream == null) throw new Exception("The log stream hasn't been setup yet!");

			//Format the message
			message = FormatMessage(message, verbosity);

			//Now add it to our messages queue, if it isn't a debug message
			if (verbosity != LogVerbosity.Debug)
				Messages.Enqueue(message);

			//Send to the Unity console if we need to
			switch (verbosity)
			{
				case LogVerbosity.Error:
					Debug.LogError(message);
					break;
				case LogVerbosity.Warn:
					Debug.LogWarning(message);
					break;
				case LogVerbosity.Debug:
				case LogVerbosity.Info:
					Debug.Log(message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(verbosity), verbosity, null);
			}

			string FormatMessage(string msg, LogVerbosity logVerbosity)
			{
				string time = GetCurrentTime();

				//The method that called it
				MethodBase callingMethod = GetCallingMethod();

				return
					$"[{time} {logVerbosity}] [{callingMethod.DeclaringType?.Name}] {msg}";
			}
		}

		#region Helper functions

		/// <summary>
		/// Returns the method that requested to log a message
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.NoInlining)] //Don't inline this, or stack traces get weird
		private static MethodBase GetCallingMethod()
		{
			return new StackFrame(3, false).GetMethod();
		}

		/// <summary>
		/// Returns a string format of the current date and time
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string GetCurrentTime()
		{
			return DateTime.Now.ToString(CultureInfo.InvariantCulture);
		}

		private static string CalculateFinalLogName()
		{
			DateTime now = DateTime.Now;

			return $"{now:yyyy-MM-dd-HH-mm-ss}.log";
		}

		#endregion
	}
}