#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Helper.Extensions;
using JetBrains.Annotations;
using Settings;
using UnityEngine;
using static Global.LogVerbosity;
using static Global.StackTraceFlags;
using DateTime = System.DateTime;
using Debug = UnityEngine.Debug;
using ThreadPriority = System.Threading.ThreadPriority;

#endregion

//TODO: Figure out MethodImpl.NoInlining for functions to make sure stack trace is right
//TODO: Create monitoring app to check when game closes what the exit code was
namespace Global
{
    public static class Logger
    {
        private const StackTraceFlags FileInfo = StackTraceFlags.FileInfo;

        /// <summary>
        /// How many stack frames we skip (so we don't include our of logging functions)
        /// </summary>
        private const int StackFramesToSkip = 2;

        /// <summary>
        ///     A string representing our message indent
        /// </summary>
        private const string Indent = "    ";

        /// <summary>
        ///     This is used to distinguish between messages that we have already sent with <see cref="Log" /> and ones sent by
        ///     other code
        ///     <para />
        ///     This helps remove duplicate messages. To ensure nice looking messages, make sure not to use any characters that
        ///     will be visible/have an impact on the message
        /// </summary>
        private const string Distinguisher = "\r\r\r\r";

        private static readonly LogNamingMode LogNamingMode = LogNamingMode.DateTime;

        /// <summary>
        ///     The path that the log file will be saved to while the game is running, will never change across runs unless the
        ///     logger undergoes a major revamp
        /// </summary>
        private static string StaticLogPath;

        /// <summary>
        ///     The final log path in which each log file is copied to at the end of each session
        /// </summary>
        //TODO: Remove this and move it to the CleanUp() function
        private static readonly string FinalLogPath = CalculateFinalLogPath();

        /// <summary>
        ///     The <see cref="StreamWriter" /> that represents our log file
        /// </summary>
        private static StreamWriter logStream;

        /// <summary>
        ///     The thread that logs our messages to the file
        /// </summary>
        private static Thread logThread;

        /// <summary>
        ///     A queue containing all of our messages that we need to write to our log
        /// </summary>
        private static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

        /// <summary>
        ///     This bool signals to our auto logger when it's time to 'shut down'
        /// </summary>
        private static bool endLog;

        /// <summary>
        ///     What information should we include in the stack trace
        /// </summary>
        public static StackTraceFlags StackTraceFlags { get; set; } = 0;

        /// <summary>
        ///     Should we send log messages to the unity console as well?
        /// </summary>
        public static bool SendToUnityDebug { get; set; } = true;

        /// <summary>
        /// Should Unity Debug messages be intercepted and added to our log?
        /// </summary>
        public static bool InterceptUnityDebug { get; set; }

        /// <summary>
        /// How verbose should our log messages be
        /// </summary>
        public static LogVerbosity Verbosity { get; set; } = VERBOSE;


        //We use this to initialize the class
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            //Now create the new log file, as long as we don't already have it open
            if (logStream == null)
            {
                Directory.CreateDirectory(Logging.LogSaveDirectory);
                //This is to avoid errors when there are multiple exes running at the same time e.g. editor and build
                //TODO: Fix this. It seems to duplicate the path

                string filename = $"Latest{Logging.LogFileExtension}";
                int attempt = 0;
                //A list of the exceptions that happened when trying to create the log file
                List<IOException> exceptions = new List<IOException>(); //TODO: Make these get logged once initiialized
                while (true)
                {
                    //Success won't be set to true until we manage to create a file (if an exception is thrown, we'll never get to that stage_
                    try
                    {
                        logStream = File.CreateText(Path.Combine(Logging.LogSaveDirectory, filename));
                        //We've succeeded by now, so update our log path and break our of the loop
                        StaticLogPath = ((FileStream) logStream.BaseStream).Name;
                        break;
                    }
                    //This catches any sharing violations. https://stackoverflow.com/questions/425956/how-do-i-determine-if-an-ioexception-is-thrown-because-of-a-sharing-violation#comment47286987_25695342
                    catch (IOException e) when ((e.HResult & 0xFFFF) == 32)
                    {
                        attempt++; //Each time there's an error, increment the attempt number
                        exceptions.Add(e);
                        //Include the exception name and the attempt in the filename
                        filename = $"Latest_Error_Sharing_Violation_{attempt}{Logging.LogFileExtension}";
                    }
                }

                logStream.AutoFlush = true;
            }
            //If we already have a log stream, just log a message and return
            else
            {
                Log("Log stream already open...", INFO);
                return;
            }

            //Now we set up our disposer to execute when the game is closed or the editor stops playing
            Application.quitting += CleanUp;

            //Create a new thread for our auto message writer
            logThread = new Thread(AutoWriteMessages)
            {
                Name = "Logging thread",
                //We don't need a high priority thread, and it's going to be a background task
                Priority = ThreadPriority.Lowest, IsBackground = true
            };

            //And start it
            logThread.Start();

            //Print some system info
            PrintSystemInfo();

            //Set up a logger to automatically intercept unity messages
            Application.logMessageReceivedThreaded += ApplicationOnLogMessageReceivedThreaded;
        }

        /// <summary>
        ///     Called every time a unity message is received
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stacktrace"></param>
        /// <param name="type"></param>
        private static void ApplicationOnLogMessageReceivedThreaded(string message, string stacktrace, LogType type)
        {
            //First, check that we're supposed to be intercepting Unity messages
            if (!InterceptUnityDebug)
                return;
            //Also check that the message isn't one that we already sent, but intercepted by unity
            if (message.Substring(0, Distinguisher.Length) == Distinguisher)
                return;
            //Indent our stacktrace
            stacktrace = stacktrace.Replace("\n\r", "\n").Replace("\n", "\n" + Indent);
            //And our message
            message = message.Replace("\n\r", "\n").Replace("\n", "\n" + Indent);

            //Format our message
            string output = $@"[{GetCurrentTime()}] Unity message ({type}):
{Indent}{message}
{Indent}{stacktrace}";
            Messages.Enqueue(output);
        }

        /// <summary>
        ///     Prints a summary of system information to help with debugging
        /// </summary>
        private static void PrintSystemInfo()
        {
            //Create a string builder with a starting size of our verbosity
            StringBuilder builder = new StringBuilder(1024);
            //We don't use Log(), because we don't want the extra info.
            //Instead, we use a string builder and add info depending on the selected verbosity

            builder.Append($@"=====  Log file for {Application.productName}  =====

Current time:           {GetCurrentTime()}
Command line arguments:	""{string.Join(" ", Environment.GetCommandLineArgs())}""
Log verbosity:          {Verbosity}
Stack trace flags:      {StackTraceFlags}

=====  Executable info  =====
Version:                {Application.version}
Genuine:                {(Application.genuineCheckAvailable ? Application.genuine.ToString() : "Unknown") /*Return "Unknown" if we can't check the validity*/}
Running as 64-Bit:      {Environment.Is64BitProcess}");
            if (CheckVerbosity(DEBUG))
                builder.Append($@"
Unity version:          {Application.unityVersion}
CLR Version:            {Environment.Version.ToString(2) /*Only include the major and minor revisions*/}
Build GUID:             {(Application.buildGUID == string.Empty ? "null" : Application.buildGUID)}
Persistent data path:   {Application.persistentDataPath}
Data path:              {Application.dataPath}
Installer name:         {(Application.installerName == string.Empty ? "null" : Application.installerName)}
Installer mode:         {Application.installMode}");
            if (CheckVerbosity(VERBOSE))
                builder.Append($@"
Target frame rate:      {Application.targetFrameRate}");

            builder.Append($@"

=====  Device info      =====
Platform:               {Application.platform.ToString()}
OS:                     {SystemInfo.operatingSystem}");
            if (CheckVerbosity(VERBOSE))
                builder.Append($@"
CPU:                    {SystemInfo.processorType} {SystemInfo.processorCount} core(s)
GPU:                    {SystemInfo.graphicsDeviceName}");
            builder.Append($@"
Graphics API:           {SystemInfo.graphicsDeviceVersion}");

            Process process = Process.GetCurrentProcess();
            builder.Append($@"

=====  Runtime info     =====
Process ID:             {process.Id}
Process name:           {process.ProcessName}
Process priority:       {process.PriorityClass}");

            builder.AppendLine()
                .AppendLine(); //Add a line ending so we have some space before the actual log messages start

            Messages.Enqueue(builder.ToString()); //And add it to our messages queue
        }

        /// <summary>
        ///     This automatically writes our log messages to our stream writer in a thread safe manner
        /// </summary>
        private static void AutoWriteMessages()
        {
            endLog = false;

            //Loop until we need to quit
            while (!endLog)
                //If we have a message to write, dequeue it
                if (Messages.TryDequeue(out string message))
                    //And write it to stream
                    WriteDirect(message);

            //Write any remaining messages
            while (!Messages.IsEmpty)
            {
                Messages.TryDequeue(out string message);
                WriteDirect(message);
            }

            WriteDirect($"\nLog file ended at {GetCurrentTime()}\n=====  End of log file  =====");
            endLog = false;

            //Close the stream
            logStream.Dispose();

            //Now copy our 'latest.log' to it's final destination
            File.Copy(StaticLogPath, FinalLogPath, false);

            Debug.Log("Logger closing...");

            void WriteDirect(string str)
            {
                logStream.WriteLine(str);
            }
        }

        #region Cleanup code

        /// <summary>
        ///     This cleans up any left over resources
        /// </summary>
        private static void CleanUp()
        {
            //Signal to our logging thread that it's time to end
            endLog = true;

            //And stop intercepting unity debug messages
            Application.logMessageReceivedThreaded -= ApplicationOnLogMessageReceivedThreaded;
        }

        #endregion

        #region Log functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)] //Inline where possible
        public static void Log<T>([CanBeNull] T obj, LogVerbosity verbosity)
        {
            Log(obj?.ToString(), verbosity);
        }

        /// <summary>
        ///     Logs a string to the log file, adding extra information such as stack traces and caller names
        /// </summary>
        /// <param name="message">The string to log to file</param>
        /// <param name="verbosity">
        ///     How verbose is this message. If the message's verbosity is higher than that of the log file's,
        ///     the message will be ignored.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)] //Don't inline, to preserve stack traces
        public static void Log([CanBeNull] string message, LogVerbosity verbosity)
        {
            message = message ?? "null";

            //Check the log stream isn't null
            if (logStream == null)
            {
                Init();
                MethodBase caller = GetCallingMethod();
                Log(
                    $"Log requested before log stream was initialised [{(caller.DeclaringType != null ? $"{caller.DeclaringType.FullName}." : string.Empty)}{caller.Name}]\n",
                    WARN);
            }

            //Now check the stream hasn't been closed (check the base stream isn't null)
            if (logStream?.BaseStream == null)
            {
                //Reinitialise it and log a warning
                Init();
                Log("Log stream unexpectedly closed. Reopening...\n", ERROR);
            }

            //If our verbosity isn't high enough, quit
            if (!CheckVerbosity(verbosity))
                return;

            //Format the message
            message = FormatMessage(message, verbosity);

            //Now add it to our messages queue
            Messages.Enqueue(message);

            //Send to the unity console if we need to
            if (SendToUnityDebug)
                switch (verbosity)
                {
                    case ERROR:
                        Debug.LogError(message);
                        break;
                    case WARN:
                        Debug.LogWarning(message);
                        break;
                    case INFO:
                    case DEBUG:
                    case VERBOSE:
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
                StringBuilder stackTraceBuilder = new StringBuilder();

                //Get a stack trace
                StackFrame[] stackFrames = new StackTrace(StackFramesToSkip, HasStackFlag(FileInfo)).GetFrames();
                if (stackFrames != null)
                    for (int f = 0; f < stackFrames.Length; f++)
                    {
                        StackFrame frame = stackFrames[f];
                        MethodBase method = frame.GetMethod();

                        //If the method belongs to a Unity function but we aren't including them, log a message and skip the rest of the trace
                        if ((method.DeclaringType != null) && (method.DeclaringType.Namespace != null) &&
                            !HasStackFlag(IncludeUnity) && method.DeclaringType.Namespace.StartsWith("Unity"))
                        {
                            stackTraceBuilder.AppendFormat(
                                "{0} === {1} Unity functions hidden. To show, enable the '{2}' flag in '{3}' ===\n",
                                Indent,
                                stackFrames.Length - f, nameof(IncludeUnity), nameof(StackTraceFlags));
                            break;
                        }

                        //Indent our line
                        stackTraceBuilder.Append(Indent).Append("at ");

                        //If we should include the namespace
                        if (HasStackFlag(Namespace) &&
                            //Check the namespace isn't the global namespace (null)
                            !string.IsNullOrEmpty(method.DeclaringType?.Namespace))
                            stackTraceBuilder.Append(method.DeclaringType.Namespace).Append('.');
                        //Add the class name
                        stackTraceBuilder.Append(method.DeclaringType?.Name).Append('.');
                        //And the method
                        stackTraceBuilder.Append(method.Name);

                        //Add parameter info
                        if (HasStackFlag(ParameterTypes) || HasStackFlag(ParameterNames))
                        {
                            stackTraceBuilder.Append('(');
                            //Loop over each parameter and add it's info
                            ParameterInfo[] paramInfos = method.GetParameters();
                            for (int p = 0; p < paramInfos.Length; p++)
                            {
                                //Add the type, including the namespace if necessary
                                if (HasStackFlag(ParameterTypes))
                                    stackTraceBuilder.Append(HasStackFlag(Namespace)
                                        ? paramInfos[p].ParameterType.FullName
                                        : paramInfos[p].ParameterType.Name);

                                //Add a space in between the parameter type and name if we need to
                                if (HasStackFlag(ParameterTypes) &&
                                    HasStackFlag(ParameterNames))
                                    stackTraceBuilder.Append(' ');

                                if (HasStackFlag(ParameterNames))
                                    stackTraceBuilder.Append(paramInfos[p].Name);

                                //Now add a comma to separate the values
                                if (p != paramInfos.Length - 1)
                                    stackTraceBuilder.Append(',').Append(' ');
                            }

                            stackTraceBuilder.Append(')');
                        }

                        //Append the file name and number
                        if (HasStackFlag(FileInfo))
                            stackTraceBuilder.Append($" at {frame.GetFileName()} line {frame.GetFileLineNumber()}");

                        //Add a new line
                        stackTraceBuilder.AppendLine();
                    }

                //Which thread called it
                string thread = Thread.CurrentThread.Name ?? "Unknown thread";

                return
                    $"{Distinguisher}[{time}] [{thread}] [{callingMethod.DeclaringType?.Name}] {(CheckVerbosity(DEBUG) ? $"\n{Indent}" : " ")}{logVerbosity}: {msg}\n{stackTraceBuilder}";
            }
        }

        #endregion

        #region Helper functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckVerbosity(LogVerbosity verbosity)
        {
            return verbosity <= Verbosity;
        }

        /// <summary>
        ///     Returns the method that requested to log a message
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)] //Don't inline this, or stack traces get weird
        private static MethodBase GetCallingMethod()
        {
            //Skip one extra frame, because we don't want to include this method as well
            return new StackFrame(StackFramesToSkip + 1, false).GetMethod();
        }

        /// <summary>
        ///     Returns a string format of the current date and time
        /// </summary>
        /// <returns></returns>
        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        [NotNull]
        private static string CalculateFinalLogPath()
        {
            DateTime now = DateTime.Now;
            string fileName;

            switch (LogNamingMode)
            {
                //Format as 'Logs/2019-12-25 1.log', 'Logs/2019-12-25 2.log' etc
                case LogNamingMode.Incremental:
                {
                    //Get the current date
                    string dateString = now.ToString("yyyy-MM-dd");

                    //Loop until we find a free file name
                    int fileNo = 0;
                    while (true)
                    {
                        fileName = Path.Combine(Logging.LogSaveDirectory, $"{dateString} {fileNo}.log");
                        if (File.Exists(fileName)) fileNo++;
                        else break;
                    }

                    break;
                }
                //Format as 'Logs/2019-06-21 12-15-45'
                case LogNamingMode.DateTime:
                    fileName = Path.Combine(Logging.LogSaveDirectory, $"{now:yyyy-MM-dd HH-mm-ss}.log");
                    break;
                default:
                    Log($"Invalid log naming mode {LogNamingMode}", ERROR);
                    fileName = Path.Combine(Logging.LogSaveDirectory, "Log-Invalid-Naming-Mode.log");
                    break;
            }

            return fileName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasStackFlag(StackTraceFlags flag)
        {
            return (StackTraceFlags & flag) != 0;
        }

        #endregion
    }
}