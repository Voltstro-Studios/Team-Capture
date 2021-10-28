// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Team_Capture.Logging
{
    internal class TCUnityLogger : ILogger
    {
        public bool IsLogTypeAllowed(LogType logType)
        {
            return true;
        }

        public void Log(LogType logType, object message)
        {
            switch (logType)
            {
                case LogType.Error:
                    Logger.Error(message.ToString());
                    break;
                case LogType.Warning:
                    Logger.Warn(message.ToString());
                    break;
                case LogType.Assert:
                case LogType.Log:
                    Logger.Info(message.ToString());
                    break;
                case LogType.Exception:
                    Logger.Error(message.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void Log(LogType logType, object message, Object context)
        {
            Log(logType, message);
        }

        public void Log(LogType logType, string tag, object message)
        {
            switch (logType)
            {
                case LogType.Error:
                    Logger.Error($"{tag} {message}");
                    break;
                case LogType.Warning:
                    Logger.Warn($"{tag} {message}");
                    break;
                case LogType.Assert:
                case LogType.Log:
                    Logger.Info($"{tag} {message}");
                    break;
                case LogType.Exception:
                    Logger.Error($"{tag} {message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void Log(LogType logType, string tag, object message, Object context)
        {
            Log(logType, tag, message);
        }

        public void Log(object message)
        {
            Logger.Info(message.ToString());
        }

        public void Log(string tag, object message)
        {
            Logger.Info($"{tag} {message}");
        }

        public void Log(string tag, object message, Object context)
        {
            Log(tag, message);
        }

        public void LogWarning(string tag, object message)
        {
            Logger.Warn($"{tag} {message}");
        }

        public void LogWarning(string tag, object message, Object context)
        {
            LogWarning(tag, message);
        }

        public void LogError(string tag, object message)
        {
            Logger.Error($"{tag} {message}");
        }

        public void LogError(string tag, object message, Object context)
        {
            LogError(tag, message);
        }

        public void LogException(Exception exception)
        {
            Logger.Error(exception, "An exception occurred!");
        }

        public void LogException(Exception exception, Object context)
        {
            LogException(exception);
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Error:
                    LogError("", format);
                    break;
                case LogType.Assert:
                    Log(format);
                    break;
                case LogType.Warning:
                    LogWarning("", format);
                    break;
                case LogType.Log:
                    Log(format);
                    break;
                case LogType.Exception:
                    LogError("", format);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            LogFormat(logType, format, args);
        }

        public ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get; set; }
    }
}