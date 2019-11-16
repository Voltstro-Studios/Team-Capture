using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helper;
using HelperClasses.Logger;
using SceneManagement;
using Settings;
using UnityEditor;
using UnityEngine;
using static Settings.GameSettings;
using Debug = UnityEngine.Debug;

namespace Logger
{
	/// <summary>
	/// A class that provides functionality to provide advanced loggin both inside and outside of the unity editor.
	/// </summary>
	public static class Logger
	{
		private static FileStream LogStream { get; set; } = null;
		private static DirectoryInfo logDirectory;
//		private static Queue<CustomLogMessage> CustomLogMessages { get; set; } = new Queue<CustomLogMessage>();
		private static Queue<UnityLogMessage> UnityLogMessages { get; set; } = new Queue<UnityLogMessage>();
		private static Queue<string> SimpleMessages { get; set; } = new Queue<string>();

		//If we're in the editor, we need to run this on play, so initialize on scene load
		//The assemblies are never really loaded
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
		//Otherwise we initialize it once the assemblies have been loaded
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] //Run this as soon as we can after the game starts
#endif
		internal static void Initialize ()
		{
			//We need to check if the settings have been loaded yet, or if they're on their default values
			if (!HasBeenLoaded)
			{
				//Add ourselves to the settings load event
				SettingsLoaded += Initialize;
				Debug.Log("Settings not loaded yet");
				//If they're not, quit
				return;
			}
			//By now, the settings are loaded, so we can safely remove ourselves from the event
			SettingsLoaded -= Initialize;

			//Initialize our logger

			//Create a directory and file if it doesn't exist
			logDirectory = new DirectoryInfo(Logging.LogSaveDirectory);
			if (!logDirectory.Exists)
			{
				logDirectory = Directory.CreateDirectory(Logging.LogSaveDirectory);
			}

			//We want to truncate the file, and be able to read and write to it, but we only want other processes to read it

			//Close the stream if it's open
			if (!(LogStream is null))
			{
				LogStream.Close();
			}

			LogStream = new FileStream($"{logDirectory.FullName}/Log{Logging.LogFileExtension}", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

			Debug.Log($"Loading logger from thread '{Thread.CurrentThread.Name}'. Log location is {LogStream.Name}");

			//This writes a bit of info to the start of the log file
			PrintSystemInfo();


			//If we intercet unity debug messages
			if (Logging.InterceptUnityDebug)
			{
				//Add our event handler for unity messagesa
				Application.logMessageReceivedThreaded += HandleUnityDebugMessage;
			}

			//Now we start our function that logs the Queued messages to file
			Task.Run(StartAutoLog);

			//Now we set up our disposer to execute when the game is closed or the editor stops playing
			Application.quitting += CleanUp;
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		}

		private static void PrintSystemInfo ()
		{
			//Truncate the file to 0 bytes
			LogStream.SetLength(0);
			//Todo Create a log level verbosity enum, and print different system info depending on verbosity
			//Log the system information
			LogSimple($@"
=====  Log file for {Application.productName}  =====
Current time is         {DateTime.Now}
Command line arguments:	{string.Join(", ", Environment.GetCommandLineArgs())}

=====  Executable info  =====
Version:                {Application.version}
Unity version:          {Application.unityVersion}
Build GUID:             {Application.buildGUID ?? "null"}
Persistent data path:   {Application.persistentDataPath}
Data path:              {Application.dataPath}
Installer name:         {Application.installerName ?? "null"}
Installer mode:         {Application.installMode}
Target frame rate:      {Application.targetFrameRate}
Genuine:                {(Application.genuineCheckAvailable ? Application.genuine.ToString() : "Unknown") /*Return "Unknown" if we can't check the validity*/}
Running as 64-Bit:      {Environment.Is64BitProcess}

=====  Device info      =====
Platform:               {Application.platform.ToString()}
OS:                     {SystemInfo.operatingSystem}
Version:                {Environment.Version.ToString(2)/*Only include the major and minor revisions*/}
Language:               {Application.systemLanguage}
CPU:                    {SystemInfo.processorType} {SystemInfo.processorCount} core(s) @ {SystemInfo.processorFrequency}
GPU:                    {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceVersion})
64-Bit OS:              {Environment.Is64BitOperatingSystem}
");
		}
		#region Cleanup code
#if UNITY_EDITOR
		private static void OnPlayModeStateChanged (PlayModeStateChange mode)
		{
			//If stopping play, clean up
			if (mode == PlayModeStateChange.ExitingPlayMode)
			{
				CleanUp();
			}
		}
		//Todo create a crash reporter/handler
		//AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		//private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e) => throw new NotImplementedException();
#endif
		private static void CleanUp ()
		{
			//Release our file
			LogStream.Close();
		}
		#endregion

		//This logs any messages to file
		//Todo Add support for windows event viewer logs, and if directory is unavailabe, reset to default.
		//Todo Create a new FileManager class to handle all file IO, and add a lockfile to our saved folder
		/// <summary>
		/// This logs any queued messages to file. We use a separate task so that we can log large messages without slowing down the main thread.
		/// </summary>
		private static async void StartAutoLog ()
		{
			//Make this run forever
			while (true)
			{
//				//If we have a custom message queued up
//				if (CustomLogMessages.Count != 0)
//				{
//					//Get the next message
//					CustomLogMessage message = CustomLogMessages.Dequeue();
//					//Write the message to file
//					await LogStream.WriteStringAsync(message.Message);
//				}
				//If we have a Unity message
				if (UnityLogMessages.Count != 0)
				{
					//Get the next message
					UnityLogMessage message = UnityLogMessages.Dequeue();
					string msg =
						$@"Unity Debug.{message.LogType} message:

{message.Message}

{message.StackTrace}
";
					//Indent any unity messages
					msg = msg.Replace("\n", "\t\n");
					//Write the message to file
					await LogStream.WriteStringAsync(msg);
				}
				//If we have a simple (string only) message
				if (SimpleMessages.Count != 0)
				{
					//Get the message
					string message = SimpleMessages.Dequeue();
					//Write it to file
					await LogStream.WriteStringAsync(message);
				}
				/*Todo make this log in the format
				[{Time} {Date} {Calling Class}]: {Message}
				e.g. [2:56PM 25/07/19 SingletonSettings]: Loading setting Input
				*/
			}
		}
		//This just logs a message with no stacktrace or anything like that
		public static void LogSimple (string message) => SimpleMessages.Enqueue(message);
		public static void LogSimple (object obj) => LogSimple(obj.ToString());

		//This handles unity debug messages
		private static void HandleUnityDebugMessage (string message, string stackTrace, LogType type)
		{
			UnityLogMessages.Enqueue(new UnityLogMessage(message, stackTrace, type));
			return;
		}
	}
}