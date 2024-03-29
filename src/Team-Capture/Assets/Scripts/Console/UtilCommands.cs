﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team_Capture.Core;
using Team_Capture.Logging;

namespace Team_Capture.Console
{
    /// <summary>
    ///     A bunch of util commands
    /// </summary>
    internal static class UtilCommands
    {
        [ConCommand("quit", "Quits the game")]
        public static void QuitGameCommand(string[] args)
        {
            Game.QuitGame();
        }

        [ConCommand("echo", "Echos back what you type in")]
        public static void EchoCommand(string[] args)
        {
            Logger.Info(string.Join(" ", args));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        [ConCommand("exception", "Manually causes an exception", CommandRunPermission.Both, 0, 1000)]
        public static void ManualExceptionCommand(string[] args)
        {
            try
            {
                string message = "Manual exception!";
                string argsJoined = string.Join(" ", args);
                if (!string.IsNullOrWhiteSpace(argsJoined))
                    message = argsJoined;

                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Manual exception thrown!");
            }
        }

        [ConCommand("exception_async", "Manually causes an exception (Async)")]
        public static void ManualExceptionAsyncCommand(string[] args)
        {
            try
            {
                FailingMethodAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Manual exception thrown!");
            }
        }

        private static async Task<int> FailingMethodAsync()
        {
            return await Task.FromResult(FailingEnumerator().Sum());
        }

        private static IEnumerable<int> FailingEnumerator()
        {
            yield return 1;
            throw new Exception();
        }

#endif
    }
}