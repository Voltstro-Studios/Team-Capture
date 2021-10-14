// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

#if UNITY_STANDALONE_WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Team_Capture.Core;
using UnityEngine;
using UnityEngine.Scripting;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console
{
	/// <summary>
	///     Console system for Windows
	/// </summary>
	[Preserve]
    internal class ConsoleWindows : IConsoleUI
    {
        private enum GenericAccessRights : uint
        {
            GenericWrite = 0x40000000,
            GenericRead = 0x80000000
        }
        
        private enum FileShare : uint
        {
            Read = 0x00000001,
            Write = 0x00000002
        }

        private enum CreationDisposition : uint
        {
            OpenExisting = 0x00000003
        }
        
        private enum FileFlagAttribute : uint
        {
            AttributeNormal = 0x80
        }

        private readonly string consoleTitle;

        private readonly SynchronizationContext unityThread;
        private bool isRunning;

        internal ConsoleWindows(string consoleTitle)
        {
            this.consoleTitle = consoleTitle;

            unityThread = SynchronizationContext.Current;
        }

        public void Init()
        {
            Debug.unityLogger.logEnabled = false;

            //Attempt to alloc a console for us
            if (!AllocConsole())
            {
                Logger.Error("Failed to allocate a windows console!");
                Game.QuitGame();
            }
                
            SetConsoleTitle(consoleTitle);
            
            //Setup our console streams
            System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()){ AutoFlush = true});
            InitializeInStream();
            
            //Start input system
            isRunning = true;
            _ = Task.Run(HandleInputs);

            Logger.Info("Started Windows command line console.");
        }

        public void Shutdown()
        {
            isRunning = false;
        }

        public void LogMessage(string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.Exception:
                case LogType.Error:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(message);
                    System.Console.ResetColor();
                    break;
                case LogType.Warning:
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(message);
                    System.Console.ResetColor();
                    break;
                case LogType.Assert:
                case LogType.Log:
                    System.Console.WriteLine(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void UpdateConsole()
        {
        }

        public bool IsOpen()
        {
            return true;
        }

        private Task HandleInputs()
        {
            while (isRunning)
            {
                string input = System.Console.ReadLine();
                unityThread.Post(state => ConsoleBackend.ExecuteCommand(input), null);
            }

            return Task.CompletedTask;
        }

        //"Borrowed" from: https://stackoverflow.com/a/48864902 

        private static void InitializeInStream()
        {
            FileStream fs = CreateFileStream("CONIN$", GenericAccessRights.GenericRead, FileShare.Read, FileAccess.Read);
            if (fs != null) System.Console.SetIn(new StreamReader(fs));
        }

        private static FileStream CreateFileStream(string name, GenericAccessRights desiredAccess, FileShare shareMode,
            FileAccess dotNetFileAccess)
        {
            SafeFileHandle file =
                new SafeFileHandle(
                    CreateFileW(name, desiredAccess, shareMode, IntPtr.Zero, CreationDisposition.OpenExisting,
                        FileFlagAttribute.AttributeNormal, IntPtr.Zero), true);
            if (file.IsInvalid) return null;

            FileStream fs = new FileStream(file, dotNetFileAccess);
            return fs;
        }

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleTitle", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetConsoleTitle(string title);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
            string lpFileName,
            GenericAccessRights dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            FileFlagAttribute dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );
    }
}

#endif