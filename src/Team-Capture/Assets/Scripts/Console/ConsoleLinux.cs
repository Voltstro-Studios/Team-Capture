// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

#if UNITY_STANDALONE_LINUX
using System;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console
{
	/// <summary>
	///     Console system for Linux
	/// </summary>
	internal class ConsoleLinux : IConsoleUI
    {
        private readonly string consoleTitle;
        private string currentLine;

        private string previousTitle;

        internal ConsoleLinux(string consoleTitle)
        {
            Debug.unityLogger.logEnabled = false;
            this.consoleTitle = consoleTitle;
        }

        public void Init()
        {
            System.Console.Clear();
            currentLine = "";

            previousTitle = System.Console.Title;
            System.Console.Title = consoleTitle;

            Logger.Info("Started Linux command line console.");
        }

        public void Shutdown()
        {
            System.Console.Title = previousTitle;
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
            //Return if there is no key available
            if (!System.Console.KeyAvailable)
                return;

            //Read the key
            ConsoleKeyInfo keyInfo = System.Console.ReadKey();
            switch (keyInfo.Key)
            {
                //Enter in input
                case ConsoleKey.Enter:
                    ConsoleBackend.ExecuteCommand(currentLine);
                    currentLine = "";

                    break;

                //Remove last input
                case ConsoleKey.Backspace:
                    if (currentLine.Length > 0)
                    {
                        currentLine = currentLine.Substring(0, currentLine.Length - 1);
                        System.Console.SetCursorPosition(0, System.Console.BufferHeight - 1);
                        ClearLine();
                        System.Console.Write(currentLine);
                    }

                    break;

                //Enter in key char
                default:
                    currentLine += keyInfo.KeyChar;
                    break;
            }
        }

        public bool IsOpen()
        {
            return true;
        }

        private static void ClearLine()
        {
            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
            System.Console.CursorLeft = 0;
        }
    }
}

#endif