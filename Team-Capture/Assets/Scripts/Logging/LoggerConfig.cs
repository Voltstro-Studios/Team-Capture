﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Core;

namespace Team_Capture.Logging
{
    /// <summary>
    ///     The config for <see cref="Logger" />
    /// </summary>
    public sealed class LoggerConfig
    {
        /// <summary>
        ///     The underlying stream will be permit to do buffered writes
        /// </summary>
        public bool BufferedFileWrite = true;

        /// <summary>
        ///     The directory to log files to
        /// </summary>
        public string LogDirectory = Game.GetGameExecutePath() + "/Logs/";

        /// <summary>
        ///     The format the the files will use
        /// </summary>
        public string LogFileDateTimeFormat = "yyyy-MM-dd-HH-mm-ss";
    }
}